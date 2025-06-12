#!/bin/bash

# Get the latest tag in the format vX.Y.Z
latest_tag=$(git tag --sort=-v:refname | grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' | head -n 1)

if [ -z "$latest_tag" ]; then
  echo "No valid tags found. Please ensure a tag like vX.Y.Z exists."
  exit 1
fi

echo "Latest tag: $latest_tag"

# Extract version components
version=${latest_tag#v}               # remove leading 'v'
IFS='.' read -r major minor patch <<< "$version"

# Increment the patch version
new_patch=$((patch + 1))
new_tag="v$major.$minor.$new_patch"

echo "Creating new tag: $new_tag"

# Create and force-push the new tag
git tag -f "$new_tag"
git push origin "$new_tag" --force

echo "Tag $new_tag pushed successfully."