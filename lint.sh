#!/bin/bash
CMD=$1

if [[ $CMD = "check" ]]; then
    dotnet format --verify-no-changes
    dotnet csharpier check .
elif [[ $CMD = "fix" ]]; then
    dotnet format
    dotnet csharpier format .
    echo ''
    echo 'Done. Note that some manual fixes may be required.'
else
    echo usage: "$0 [check|fix]"
fi
