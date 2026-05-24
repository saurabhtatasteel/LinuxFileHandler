#!/bin/bash

# Make the script executable
# chmod +x copyfile.sh

# Source file
SOURCE="/home/user/source/file.txt"

# Destination path
DESTINATION = "/home/user/backup/file.txt"

# Copy file
cp "$SOURCE" "$DESTINATION"

echo 0
