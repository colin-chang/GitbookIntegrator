#!/bin/bash

cd $1
git add -A
git commit -m $2
git pull
git push