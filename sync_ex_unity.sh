rm -rf ./core/Assets/exUnity/
rsync -Cavz --exclude=".git*" --exclude=".DS_Store" --exclude="*.exvim" ~/exdev/ex-unity/core/Assets/exUnity/ ./core/Assets/exUnity/
