import os

rep = (
    '_Clip.anim',
    '.anim',
)

folder = 'Animations'

for path, dirs, files in os.walk(folder):
    for file in files:
        newfile = file.replace(rep[0], rep[1])
        print(newfile)

        if rep[0] in file:
            os.rename(path + '/' + file, path + '/' + newfile)
