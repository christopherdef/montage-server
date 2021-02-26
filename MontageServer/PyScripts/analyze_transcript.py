import sys
from os import path
import random
import time
import json
import tempfile
import numpy as np

from pyBTM import pyBTM

import spacy
import string
from nltk.corpus import stopwords

from nltk.stem.porter import PorterStemmer
# load spacy stuff
stops = set(stopwords.words('english'))
punc = set(string.punctuation)

nlp = spacy.load('en_core_web_sm')
nlp.disable_pipes('parser', 'ner')


END = '\n\n'

class Response:
    def __init__(self, _id):
        self.projectid = _id
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
    
    response.transcript = open(transcript_pt, 'r', encoding='utf8').read()
    response.topics =   get_topics(response.transcript)
    response.individuals = ["a", "b"]
    response.objects = ["o1", "o2"]
    response.sentiments = [0.1, 0.3]

    return response

def get_topics(transcript):
    pp_txt = []
    for line in transcript.split('\n'):
        pp_txt.append(' '.join(preprocess(line)))

    tmpf = tempfile.NamedTemporaryFile(mode='r+', delete=False, encoding='utf8')
    tmpf.write(('\n'.join(pp_txt)))
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

def main(_id, transcript_pt):
    '''
    Driver transcript analysis
    Communicates results to calling process via stdout
    '''
    # silence all output
    stdout = sys.stdout
    sys.stdout = None

    response = Response(_id)

    # analyze transcript
    #time.sleep(2) # simulate processing time 
    response = analyze_transcript(response, transcript_pt)

    # send analysis back to caller
    sys.stdout = stdout
    serialized_response = json.dumps(response.__dict__)
    print(serialized_response+END)
    sys.stdout.flush()

    # TODO: clean up audio file (delete?)

if __name__=="__main__":
    _id = sys.argv[1]
    transcript_pt = sys.argv[2]
    main(_id, transcript_pt)