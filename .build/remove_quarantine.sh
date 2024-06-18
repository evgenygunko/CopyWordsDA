#!/bin/bash

# Check if directory parameter is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <directory>"
    exit 1
fi

DIRECTORY="$1"

# Check if the provided parameter is a valid directory
if [ ! -d "$DIRECTORY" ]; then
    echo "Error: $DIRECTORY is not a valid directory."
    exit 1
fi

# Check if xattr is available on the system
if ! command -v xattr &> /dev/null; then
    echo "xattr command not found. Please install it to use this script."
    exit 1
fi

# Iterate over all files in the specified directory
for file in "$DIRECTORY"/*; do
    # Check if the file has the quarantine attribute
    if xattr -p com.apple.quarantine "$file" &> /dev/null; then
        # Remove the quarantine attribute
        xattr -d com.apple.quarantine "$file"
        echo "Removed quarantine attribute from $file"
    else
        echo "No quarantine attribute found on $file"
    fi
done

echo "Script completed."
