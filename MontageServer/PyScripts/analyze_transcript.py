import sys
import speech_recognition as sr
from os import path
import random
import time
import json

# TODO: import numpy fails!
# import numpy as np

END = '\n\n'

class Response:
    def __init__(self, _id):
        self.reqid = _id
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
    
    response.transcript = open(transcript_pt, 'r').read()
    response.topics =   [[1,2,3],[34, 10,2]]
    response.individuals = ["a", "b"]
    response.objects = ["o1", "o2"]
    response.sentiments = [0.1, 0.3]

    return response

def main(_id, transcript_pt):
    '''
    Driver transcript analysis
    Communicates results to calling process via stdout
    '''
    response = Response(_id)

    # analyze transcript
    time.sleep(2) # simulate processing time 
    response = analyze_transcript(response, transcript_pt)

    # TODO: send analysis back to caller
    serialized_response = json.dumps(response.__dict__)
    print(serialized_response+END)
    sys.stdout.flush()

    # TODO: clean up audio file (delete?)

if __name__=="__main__":
    _id = sys.argv[1]
    transcript_pt = sys.argv[2]
    main(_id, transcript_pt)