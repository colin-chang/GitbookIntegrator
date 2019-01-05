#!/bin/bash

dotnet build
cd bin/Debug/netcoreapp2.2/
dotnet ./Integrator.dll