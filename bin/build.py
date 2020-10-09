#!/usr/bin/python -B
import gdown
import logging
import os
import platform
import sys


USD_BINARIES_PYTHON = {"v20.08": {"Windows": "https://drive.google.com/uc?id=1GDTMhT3GK9k0mAtJIhtB7lZ5PxW9SRVJ" ,
                                 "Linux": "",
                                 "Darwin":""}}
USD_BINARIES_NO_PYTHON = {"v20.08": {"Windows": "https://drive.google.com/uc?id=1uE_a9t9fHv-ldTYZEIhnsuhiLuo2pw-O" ,
                                 "Linux": "",
                                 "Darwin":""}}

def download_binaries(usd_version, output_dir=""):
    if not os.path.exists(output_dir):
        logging.error("Target path doesn't exist: {0}".format(output_dir))
        return
        
    output_path = os.path.join(output_dir, "usd_{0}.zip".format(usd_version))
    output_path = os.path.normpath(output_path)
    logging.info("Downloading USD {0} python build to {1} ...".format(usd_version, output_path))
    gdown.download(USD_BINARIES_PYTHON[usd_version][platform.system()], output_path)
    
    output_path = os.path.join(output_dir, "usd_{0}_no_python.zip".format(usd_version))
    output_path = os.path.normpath(output_path)
    logging.info("Downloading USD {0} no python build to {1} ...".format(usd_version, output_path))
    gdown.download(USD_BINARIES_NO_PYTHON[usd_version][platform.system()], output_path)
    
if __name__ == "__main__":
    download_binaries(sys.argv[1], sys.argv[2])