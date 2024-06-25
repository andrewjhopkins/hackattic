#!/bin/bash

PASS=$(fcrackzip -u -l 4-6 -c a1 /tmp/zipped.zip | cut -c 27- | xargs)

unzip -P $PASS -d /tmp/zipped /tmp/zipped.zip
