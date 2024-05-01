#!/bin/bash
echo "Publishing RPM"

sudo alien --to-rpm --scripts --bump=0 "$1"
mv -f *.rpm ../rpm

echo "DONE"