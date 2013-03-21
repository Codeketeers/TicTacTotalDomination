TicTacTotalDomination
=====================

Group project for the spring 2013 software engineering class.

## Notes for working on features ##
When you're working on the project, make sure not to do it on the master branch. Master should be for production ready, tested code.

To start working, checkout a branch for the feature, based on the master branch.

    git checkout master
	git pull
	git checkout -b feature-some-feature

As you are completing work, make sure to commit and push so you can't lose anything.

    git commit -a
	git push

If you're pushing your feature for the first time:

    git push origin feature-some-feature

When the feature is tested and done, push the branch to github and intiate a pull request so we can all code review the change, and merge it into master.