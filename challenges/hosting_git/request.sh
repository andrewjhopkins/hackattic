#!/bin/bash
curl --header "Content-Type: application/json" \
  --request POST \
  --data '{"repo_host":"<server ip>"}' \
  https://hackattic.com/_/git/$1
