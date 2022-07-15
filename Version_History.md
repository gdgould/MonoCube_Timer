# Version History

## 0.5.0.1 (In development)

Adding cross-platform functionality...

### Features

* Cross-platform file system support added.  The program should now hopefully be functional on OSX and Linux.  THIS HAS NOT BEEN TESTED.

### Bugs

* Fixed a bug where the Logs folder would not auto-generate on a new install, causing an instant crash with no logging.

## 0.5.0.0 (July 11, 2022)

This version was the first to be ported to GitHub.  No functional changes were made to the code, just refactoring and commenting.

### Code

* Combined ScrollBox and TimeDisplayScrollBox by adding options to display Dates, Buttons (+2, DNF, delete), or neither.
* Commented everything.

# Previous Versions

Version history was not kept track of prior to 0.5.0.0.  For a general idea of added features:

## 0.4.2.0 (June 2022)

### Features

* Added support for color schemes: place a colorscheme.xml file in the storage directory (`%AppData%\Roaming\MonoCubeTimer`, `~/Library/Application Support/MonoCubeTimer`, or `home/.monoCubeTimer`) to override the default dark theme.

### Bugs

* Fixed a bug where averages would not be generated when their required number of times was reached if the program loaded from the cache.
* Fixed a bug where commas in comments messed with time csv files.

## 0.4.1.0 (January 2022)

* Fixed various file system bugs with the custom categories.
* Added the ability to delete times from previous sessions.
* Added the cache system to speed up loading by not sorting all past times every time the program loads.

## 0.4.0.0 (December 2021)

* Added support for custom cube categories.
* Added the ability to edit comments in the statistics window.

## 0.3.0.0 (August 2021)

* Added support for different cube categories.
* Added statistics windows to display information about times.

## 0.2.0.0 (July 2021)

* Saving/Loading of times, with display in the scrollboxes to either side.
* Statistics and averages displayed.

## 0.1.0.0 (July 2021)

* Basic timer functionality
