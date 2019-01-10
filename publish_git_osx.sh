#!/bin/bash

cd $1
git commit -m $2
git pull
git push