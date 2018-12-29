#!/bin/sh

pushd src/cpcontrib.logging

# CPLog

grep -r "NLog\." -l | xargs sed 's/\([[:space:]|"]\)NLog\./\1CPLog\./g' -i
grep -r "T:NLog\." -l | xargs sed 's/\(T:\)NLog\./\1CPLog\./g' -i

popd