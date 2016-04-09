#!/bin/bash
cd "$(dirname "$0")"
./Spectrum.Prism.exe ./Assembly-CSharp.dll ./Spectrum.Bootstrap.dll
echo "Press any key to continue"
read -sn 1
