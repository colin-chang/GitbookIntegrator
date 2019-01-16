#!/bin/bash

rm -f $1
rm -f deploy.sh
cd $2
git init
git add -A
git commit -m 'deploy'

git push -f git@github.com:colin-chang/$3.git master:gh-pages