# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.1-preview.2] - 2020-01-07
### Update recorder dependency
- Update dependency for unity recorder to version 2.1.0-preview.1

## [0.1.0-preview.5] - 2019-09-10
### Update recorder dependency
- Update dependency for unity recorder to version 2.0.3-preview.1

## [0.1.0-preview.3] - 2019-07-24
### Fixes and 19.3 support
- Fix to support HDRP out of experimental for 2019.3

## [0.1.0-preview.1] - 2019-05-09
### *First release of the Unity AOV Recorder*

This release depends on the Unity HD Scriptable Render Pipeline version 5.11 (2019.1) or version 6.6 for (2019.2)

*Known limitations* :
- First frame (0) sometimes is black 
- Active Camera target is not supported by HDRP
- Depth data is incorrect if the scene distance is large with HDRP version < 6.8
- PNG and JPEG files are written as linear file instead of sRGB.
- Simultaneous recording of the beauty pass and other render passes if TAA is enabled will generate artifacts 
- Recorder issue FTV-251: Timeline recording of the beauty pass will record black with HDRP
