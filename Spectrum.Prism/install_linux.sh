#!/bin/bash
cd "$(dirname "$0")"
./Spectrum.Prism.exe -t ./Assembly-CSharp.dll -s ./Spectrum.Bootstrap.dll
echo "Press any key to continue"
read -sn 1
