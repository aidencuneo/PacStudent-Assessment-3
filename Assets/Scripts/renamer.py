import os

rep = (
    '_Clip.anim',
    '.anim',
)

folder = '../Animations'

for path, dirs, files in os.walk(folder):
    for file in files:
        print(newfile := file.replace(rep[0], rep[1]))
        rep[0] in file and os.rename(path + '/' + file, path + '/' + newfile)
