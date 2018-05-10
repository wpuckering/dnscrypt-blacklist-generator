This is a simple tool which can be used to download curated lists of domains and generate blacklist (and whitelist) files that can be referenced by dnscrypt-proxy (https://github.com/jedisct1/dnscrypt-proxy). Currently it will always append new domains from any sources that it is configured to pull from, so that the lists will always grow and never shrink as it is executed consecutively.

The default configuration included is what I personally use. Be sure to only point to curated sources that you trust.

I run this as a scheduled task on a daily basis. It will automatically stop and start the dnscrypt-proxy service if configured to do so, so that your lists, once updated, will be put to use immediatey.
