
import sys
import speech_recognition as sr
from os import path
import random
import time
import json

END = '\n\n'

class Response:
    def __init__(self, _id):
        self.reqid = _id
        self.topics = []
        self.individuals = []
        self.objects = []
        self.sentiments = []
        self.transcript = ""

def transcribe_audio(response, audio_pt):
    '''
    Derives transcript from audio at audio_pt
    @param response : Response object with loaded ID
    @param audio_pt : path to audio file
    @return : Response object with loaded transcript
    '''
    AUDIO_FILE = path.join(audio_pt)

    # use the audio file as the audio source
    recognizer = sr.Recognizer()
    with sr.AudioFile(AUDIO_FILE) as source:
        audio = recognizer.record(source)  # read the entire audio file

    transcript = ""
    # recognize speech using Sphinx
    try:
        transcript = recognizer.recognize_sphinx(audio)
        # print(f"TRANSCRIPTION\n{transcript}")
    except sr.UnknownValueError:
        transcript = "Sphinx could not understand audio"
    except sr.RequestError as e:
        transcript = "Sphinx error; {0}".format(e)

    # load into the response
    response.transcript = transcript
    return response

def main(_id, audio_pt):
    '''
    Driver audio transcription
    Communicates results to calling process via stdout
    '''
    response = Response(_id)

    # analyze transcript
    time.sleep(2) # simulate processing time 
    response = transcribe_audio(response, audio_pt)

    # TODO: send analysis back to caller
    serialized_response = json.dumps(response.__dict__)
    print(serialized_response+END)
    sys.stdout.flush()

    # TODO: clean up audio file (delete?)

if __name__=="__main__":
    _id = sys.argv[1]
    transcript_pt = sys.argv[2]
    main(_id, transcript_pt)