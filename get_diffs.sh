#!/usr/bin/env bash

# this script iterates through all the commits in a project, and saves the diff between every commit to the chronological next commit

project_root=$(dirname -- "$0")

git_project_name="spreadsheet-CharlesSS07"

# instead of using for loops and variables, this line creates two columns of text out of the commit list
# the first column is the previous commit, and the second column is the new commit
# it then chops off the first and last rows of the staggered list (since they each should have a blank)
# next, it iterates throug the entire list of chronoligcally paired commits, and creates a file named
# <previous commit>-vs-<new commit>, and then pipes the git diff of the two commits in there.
paste -d " " <(cat $project_root/commits/spreadsheet-CharlesSS07.txt;echo "") <(echo "";cat $project_root/commits/spreadsheet-CharlesSS07.txt) \
	| sed "\$d" \
	| tail -n +2 \
	| xargs -I % bash -c "\
		ROW='%'; \
		FN=\$(echo \$ROW | sed 's/ /-vs-/').txt;\
		git diff \$ROW > $project_root/diffs/$git_project_name/\$FN; \
	"
