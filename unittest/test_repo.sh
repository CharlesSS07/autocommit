#!/usr/bin/env bash

source ../src/hook/diff
source ../src/hook/request
source ../src/hook/login

try_login_prompt

SCRIPT_NAME='test_repo'
SITE_MAIN="https://u1319464.wixsite.com/git-auto-commit"
API_ENDPOINT="$SITE_MAIN/_functions-dev/"

repo=$1

cd $repo

function get_random_diff {
	# get a random diff between consecutive commits in tensorflow repo
	local diff=$(mktemp /tmp/$SCRIPT_NAME.diff.XXXXXX)
	local max=$(git rev-list --all --count)
	local i=$((0 + $RANDOM % $max))
	local commit1=$(git log --pretty=format:"%H" --skip=$i --max-count=1)
	local commit2=$(git log --pretty=format:"%H" --skip=$(($i+1)) --max-count=1)
	git diff $commit1 $commit2 > $diff
	echo $diff
}

function display_real_and_generated_message {
	# show the real commit message, and the generated one for ith commit
	local i=$1
	
	local commit1=$(git log --pretty=format:"%H" --skip=$i --max-count=1)
	local commit2=$(git log --pretty=format:"%H" --skip=$(($i+1)) --max-count=1)
	
	echo '=========================== REAL ==========================='
	git log -1 -U $commit1 --pretty=format:"%s" | cat -
	# show the real commit message
	
	echo
	echo '=========================== FAKE ==========================='
	git diff $commit2 $commit1 | cat -
	local completion=$(mktemp /tmp/$SCRIPT_NAME.completion.XXXXXX)
	local response_code=$(git diff $commit1 $commit2 | generate_commit_message $completion)
	if [[ "$response_code" -ne 200 ]] ; then
		# response was not ok
		echo "Autocommit failed. Nothing written to commit message. Error($response_code):" >&2
		cat $completion >&2
		# don't exit 1, so user can still write messages
	else
		# response was ok, append completion to the commit message file
		cat $completion
	fi
	
	rm $completion
	# show generated commit message
}

display_real_and_generated_message 2
