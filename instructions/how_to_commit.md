## How to COMMIT changes

### get latest changes (IMPORTANT before making code)
``` bash
git checkout main
git pull origin main
```
- This is ensures that what your working on is in the latest one;
- Avoids merge conflicts 

### create your own branch
``` bash
git checkout -b {name of your branch}
```
- This creates a branch for (you) to push your changes
### save your changes
``` bash
git add .
git commit -m "{description of the change}"
git push origin {name of your branch}
```
- Updates your branch and push it to the repo under your branch;
- and wait for approval/review on it

