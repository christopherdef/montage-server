
import sys
from os import path
import random
import time
import json
import tempfile
import numpy as np

# diarization setup
import os, sklearn
import pyAudioAnalysis as paa
from pyAudioAnalysis.MidTermFeatures import mid_feature_extraction
from pyAudioAnalysis.audioBasicIO import read_audio_file, stereo_to_mono
from pyAudioAnalysis.audioSegmentation import labels_to_segments
from pyAudioAnalysis.audioTrainTest import normalize_features
import numpy as np
import scipy.io.wavfile as wavfile

# topic modeling setup
from pyBTM import pyBTM
import spacy
import string
from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer
stops = set(stopwords.words('english'))
punc = set(string.punctuation)
nlp = spacy.load('en_core_web_sm')
nlp.disable_pipes('parser', 'ner')

# sentiment analysis setup
from nltk.sentiment import SentimentIntensityAnalyzer
sia = SentimentIntensityAnalyzer()

END = '\n\n'

class Response:
    def __init__(self, _id):
        self.clipid = _id
        self.topics = {}
        self.individuals = []
        self.objects = []
        self.sentiments = []
        self.transcript = ""

def analyze_transcript(response, transcript_pt):
    '''
    Analyzes topics, individuals, objects, & sentiments in a transcript
    Loads results into a the given Response object and returns
    @param response : Response object with a loaded transcript
    @return : Response object with loaded topics, individuals, objects, & sentiments
    '''
    transcript_file_read = open(transcript_pt, 'r', encoding='utf8')
    response.transcript = transcript_file_read.read()
    transcript_file_read.close()
    response.topics =   get_topics(response.transcript)
    response.individuals = ["a", "b"]
    response.objects = ["o1", "o2"]
    response.sentiments = get_sentiments(response.transcript)

    return response

def main(argv):
    '''
    Driver transcript analysis
    Communicates results to calling process via stdout
    '''
    # silence all output
    stdout = sys.stdout
    sys.stdout = None
    
    argc = len(sys.argv)
    _id = sys.argv[1]
    transcript_pt = sys.argv[2]
    audio_pt = sys.argv[3] if argc > 3 else ''

    response = Response(_id)

    # analyze transcript
    response = analyze_transcript(response, transcript_pt)
    response.individuals = get_speaker_spans(audio_pt)

    # send analysis back to caller
    sys.stdout = stdout
    serialized_response = json.dumps(response.__dict__)
    print(len(serialized_response)+1)
    print(serialized_response+END)
    sys.stdout.flush()

    # TODO: clean up audio file (delete?)

## SENTIMENT ANALYSIS ##
def get_sentiments(transcript):
    sentiments = []
    
    # TODO: s(wi) = avg(s(wi), s(w_i-2, w_i-1, wi), s(w_i-1, wi, w_i+1), s(wi, w_i+1, w_i+2))
    for w in transcript.split():
        s = sia.polarity_scores(w)
        sentiments.append(s['pos'] if s['pos'] >= s['neg'] else -s['neg'])

    return sentiments

## TOPIC MODELING ##
def get_topics(transcript):
    sentences = transcript.split('\n')
    if len(transcript.split(' ')) <= 5:
        return {}
    pp_txt = []
    for line in sentences:
        pp_token = ' '.join(preprocess(line))
        if len(pp_token) > 0:
            pp_txt.append(pp_token)

    # too harsh, use softer preprocess
    if len(pp_txt) == 0:
        for line in sentences:
            pp_token = ' '.join(preprocess(line))
        if len(pp_token) > 0:
            pp_txt.append(pp_token)
    
    # still too harsh, just casefold
    if len(pp_txt) == 0:
        pp_txt = transcript.casefold().split('\n')
    
    # no topics can be found in provided text!
    if len(pp_txt) == 0:
        return {}

    # temporary attempt to use cosine sim on word vectors
    #naive_topics = {}
    #nlp_pp_txt = nlp(pp_txt)
    #sim_mat = np.array([*([*(wi.similarity(wj) for wi in nlp_pp_txt)] for wj in nlp_pp_txt)])
    

    tmpf = tempfile.NamedTemporaryFile(mode='r+', delete=False, encoding='utf8')
    # print(f"topic tmp file: {tmpf.name}", file=sys.stdout)
    tmpf.write(('\n'.join(pp_txt)))
    tmpf.flush()
    tmpf.seek(0)
    tmpf.close()

    k = 5
    alpha = 50/k
    beta = 0.01
    niter = 50
    verbose = True
    clean_on_del = False
    btm = pyBTM.BTM(K=k, input_path=tmpf.name, alpha=alpha, beta=beta, niter=niter, verbose=verbose, clean_on_del=clean_on_del)

    btm.index_documents()
    btm.learn_topics(force=False)
    btm.infer_documents(force=False)

    L = 5
    topics = btm.get_topics(include_likelihood=False, use_words=True, L=L)
    return topics

def soft_preprocess(text_line):
    '''
    Softer data preprocessing step
    '''
    # case fold
    text = text_line.casefold()

    # filter for puntuation, and stubs
    inclusion_condition = lambda w : w.is_alpha and\
                                     w not in punc and\
                                     len(w) >= 2

    # apply filter, lemmatize
    pp_txt = [*filter(lambda w : inclusion_condition(w), text)]
    return pp_txt

def preprocess(text_line):
    '''
    Simple data preprocessing step
    '''
    # case fold
    text = text_line.casefold()

    # tokenize
    tokenized_text = nlp(text)

    # filter for stopwords, puntuation, and stubs
    inclusion_condition = lambda w : w.is_alpha and\
                                     not w.is_stop and\
                                     w not in stops and\
                                     w not in punc and\
                                     len(w) >= 2

    # apply filter, lemmatize
    pp_txt = [tok.lemma_ for tok in filter(lambda w : inclusion_condition(w), tokenized_text)]
    return pp_txt


## SPEAKER DIARIZATION ##

def get_speaker_spans(fn):
    '''
    Returns list of predicted spans between speakers
    '''
    mt_step = 0.1
    st_win = 0.05
    features = _get_features(fn, mt_step, st_win)
    clusters = _get_clusters(features, k=4)
    segs, c = labels_to_segments(clusters, mt_step)
    return segs, c

def _get_features(fn, mt_step=0.1, st_win=0.05):
    '''
    Generate the feature vector for the audio located at fn
    '''
    sr, signal = read_audio_file(fn)
    if len(signal.shape) > 1:
        raise Exception(f'Single channel audio only! This audio has the shape: {signal.shape}')
    mid_step = mt_step*sr
    short_window = round(st_win*sr)
    short_step = round(st_win*sr*0.5)
    [feats, short_feats, feat_names] = paa.MidTermFeatures\
                                             .mid_feature_extraction (signal, sr, mid_window, 
                                                                      mid_step, short_window, short_step)
    (feats_norm, MEAN, STD) = normalize_features([feats.T])
    feats_norm = feats_norm[0].T
    return feats_norm

def _get_clusters(data, k=4):
    '''
    kNN cluster the provided data 
    '''
    k_means = sklearn.cluster.KMeans(n_clusters=k)
    k_means.fit(mt_feats.T)
    clusters = k_means.labels_
    return clusters

if __name__=="__main__":
    main(sys.argv)