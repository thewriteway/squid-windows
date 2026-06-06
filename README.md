![Release](https://img.shields.io/github/v/release/thewriteway/squid-windows)
![Downloads](https://img.shields.io/github/downloads/thewriteway/squid-windows/total)
![License](https://img.shields.io/github/license/thewriteway/squid-windows)
![Dependabot](https://img.shields.io/badge/dependabot-enabled-brightgreen)
![Renovate enabled](https://img.shields.io/badge/renovate-enabled-brightgreen?logo=renovatebot)
![Stars](https://img.shields.io/github/stars/thewriteway/squid-windows)

**This is an updated forked version of diladele's installer.**

It provides a MSI Windows Installer for Squid Proxy Server.

Current build is based on the latest **Squid 7.4** build for Cygwin Windows 64 bit.


Squid Windows Installer
==============
> Squid is a caching proxy for the Web supporting HTTP, HTTPS, FTP, and more. It reduces bandwidth and improves response times by caching and reusing frequently-requested web pages. Squid has extensive access controls and makes a great server accelerator. It runs on most available operating systems, including Windows and is licensed under the GNU GPL.
> <cite> <http://www.squid-cache.org>

**Installation instructions**
-----------------------------
* [Download updated forked Squid 7.4 for Windows MSI installer] in the releases page https://github.com/thewriteway/squid-windows/releases/tag/v7.4
* Run it and click "Next" button till squid is installed as a service, than configure your browser proxy to point to localhost port 3128

**Note of Caution**
Squid proxy when hosted locally on a windows machine is not compatible with ad-blocker extension: ublock Origins's "Block Outsider Intrusion into LAN" filter, it must be turned off for the proxy to render website content correctly.

**Help**
--------
Squid documentation can be found at http://www.squid-cache.org. Specific Squid questions please use one of Squid mailing lists http://www.squid-cache.org/Support/mailing-lists.html.

**Credits**
-----------
The Squid Team and diladele.



