#!/bin/bash

#currentDate="$(date '+%Y-%m-%d')"
#echo "Build Date: "$currentDate

ARCH_X86_64="x86_64"
ARCH_ARM64="arm64"

DEST_PATH="/Users/Kanbaru/Library/CloudStorage/OneDrive-个人/Documents/GameSolution/deps/fnalibs/osx"

echo "lipo SDL2..."
#lipo -create $(realpath "/usr/local/lib/libSDL2.a") $(realpath "/opt/homebrew/lib/libSDL2.a") -output $(realpath "/opt/homebrew/lib/libSDL2.a")
#lipo -create $(realpath "/usr/local/lib/libSDL2.dylib") $(realpath "/opt/homebrew/lib/libSDL2.dylib") -output $(realpath "/opt/homebrew/lib/libSDL2.dylib")
echo "check libSDL2.a: "$(lipo -archs $(realpath "/opt/homebrew/lib/libSDL2.a"))
echo "check libSDL2.dylib: "$(lipo -archs $(realpath "/opt/homebrew/lib/libSDL2.dylib"))
cp -rf $(realpath "/opt/homebrew/lib/libSDL2.dylib") "$DEST_PATH/libSDL2-2.0.0.dylib"
cp -rf $(realpath "/opt/homebrew/lib/libSDL2.dylib") "$DEST_PATH/libSDL2.dylib"

#if [[ $(lipo -archs $(realpath "/opt/homebrew/lib/libSDL2.a")) == *$ARCH_X86_64* ]]; then
#    echo "has $ARCH_X86_64."
#fi
#if [[ $(lipo -archs $(realpath "/opt/homebrew/lib/libSDL2.a")) == *$ARCH_ARM64* ]]; then
#    echo "has $ARCH_ARM64."
#fi

export CMAKE_INSTALL_MESSAGE=NEVER
export CMAKE_OSX_ARCHITECTURES="arm64;x86_64"
export CMAKE_OSX_DEPLOYMENT_TARGET="12.3"

BUILD_CONFIGURATION="Debug"

echo "build FAudio..."
cd "$FNA/lib/FAudio"
[ ! -d "bin" ] && mkdir "bin"
cd "bin"
cmake -GXcode ..
#xcodebuild -list -project FAudio.xcodeproj -scheme FAudio -configuration Debug -destination 'platform=macOS,arch=x86_64;arm64' build
xcodebuild -quiet -project FAudio.xcodeproj -scheme FAudio -configuration $BUILD_CONFIGURATION -destination 'platform=macOS,arch=arm64' build
if [[ $? == 0 ]]; then
    echo "Success build: FAudio."
    yes | cp $(realpath "./$BUILD_CONFIGURATION/libFAudio.dylib") "$DEST_PATH/libFAudio.0.dylib"
else
    echo "Failed"
fi

echo "build FNA3D..."
cd "$FNA/lib/FNA3D"
[ ! -d "bin" ] && mkdir "bin"
cd "bin"
cmake -GXcode ..
#xcodebuild -list -project FNA3D.xcodeproj -scheme FNA3D -configuration Debug -destination 'platform=macOS,arch=x86_64;arm64' build
xcodebuild -quiet -project FNA3D.xcodeproj -scheme mojoshader -configuration $BUILD_CONFIGURATION -destination 'platform=macOS,arch=arm64' build
xcodebuild -quiet -project FNA3D.xcodeproj -scheme FNA3D -configuration $BUILD_CONFIGURATION -destination 'platform=macOS,arch=arm64' build
if [[ $? == 0 ]]; then
    echo "Success build: FNA3D."
    yes | cp $(realpath "./$BUILD_CONFIGURATION/libFNA3D.dylib") "$DEST_PATH/libFNA3D.dylib"
else
    echo "Failed"
fi

echo "build Theorafile..."
cd "$FNA/lib/Theorafile"
make --jobs=$(sysctl -n hw.logicalcpu)
[ -f "libtheorafile.dylib" ] && cp -rf "libtheorafile.dylib" "$DEST_PATH/libtheorafile.dylib" && echo "copied."

echo "copy vulkan SDK..."
[ -f "$VULKAN_SDK/MoltenVK/dylib/macOS/libMoltenVK.dylib" ] && cp -rf "$VULKAN_SDK/MoltenVK/dylib/macOS/libMoltenVK.dylib" "$DEST_PATH/libMoltenVK.dylib" && echo "copied."

echo "finished."
