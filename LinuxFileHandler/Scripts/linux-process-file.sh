#!/bin/bash

#To make script executable run below command
#chmod +x scripts/linux-process-file.sh

SOURCE_FILE="$1"
DESTINATION_PATH="$2"

echo "Source: $SOURCE_FILE"
echo "Destination: $DESTINATION_PATH"

if [ ! -f "$SOURCE_FILE" ]; then
    echo "ERROR: Source file does not exist."
    exit 1
fi

mkdir -p "$(dirname "$DESTINATION_PATH")"

cp "$SOURCE_FILE" "$DESTINATION_PATH"

if [ $? -eq 0 ]; then
    echo "File copied successfully."
    exit 0
else
    echo "ERROR: File copy failed."
    exit 2
fi