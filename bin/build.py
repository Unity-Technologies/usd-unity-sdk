#!/usr/bin/python -B
import gdown
import logging
import os
import platform
import sys
import shlex
import subprocess

USD_BINARIES_PYTHON = {"v20.08": {"Windows": "https://drive.google.com/uc?id=1GDTMhT3GK9k0mAtJIhtB7lZ5PxW9SRVJ" ,
                                 "Linux": "",
                                 "Darwin":""}}
USD_BINARIES_NO_PYTHON = {"v20.08": {"Windows": "https://drive.google.com/uc?id=1uE_a9t9fHv-ldTYZEIhnsuhiLuo2pw-O" ,
                                 "Linux": "",
                                 "Darwin":""}}

def usd_python_dirname(usd_version):
    return "usd-{0}".format(usd_version)
    
def usd_no_python_dirname(usd_version):
    return "usd-{0}_no_python".format(usd_version)
    
def download_usd_python_binaries(usd_version, output_dir=""):
    if not os.path.exists(output_dir):
        logging.error("Target path doesn't exist: {0}".format(output_dir))
        return
        
    output_path = os.path.join(output_dir, usd_python_dirname(usd_version))
    output_path = os.path.normpath(output_path)
    if os.path.exists(output_path):
        logging.info("USD {0} python build found.".format(usd_version))
        return output_path
        
    logging.info("Downloading USD {0} python build to {1} ...".format(usd_version, output_path))
    gdown.download(USD_BINARIES_PYTHON[usd_version][platform.system()], output_path+".zip")
    # TODO: unzip
    return output_path

def download_usd_no_python_binaries(usd_version, output_dir=""):
    if not os.path.exists(output_dir):
        logging.error("Target path doesn't exist: {0}".format(output_dir))
        return
        
    output_path = os.path.join(output_dir, usd_python_dirname(usd_version))
    output_path = os.path.normpath(output_path)
    if os.path.exists(output_path):
        logging.info("USD {0} no python build found.".format(usd_version))
        return output_path
        
    logging.info("Downloading USD {0} no python build to {1} ...".format(usd_version, output_path))
    gdown.download(USD_BINARIES_NO_PYTHON[usd_version][platform.system()], output_path+".zip")
    # TODO: unzip
    return output_path
    
if __name__ == "__main__":
    usd_python_dir_path = download_usd_python_binaries(sys.argv[1], sys.argv[2])
    usd_no_python_dir_path = download_usd_no_python_binaries(sys.argv[1], sys.argv[2])
    unity_version = sys.argv[3]
    
    if not os.path.exists("./build"):
        os.path.make_dir("build")
        
    cmake_cmd = "cmake -S . -B build "
    "-DPXR_USD_LOCATION={0} " 
    "-DPXR_USD_LOCATION_PYTHON_BUILT={1} "
    "-DUNITY_VERSION={2} "
    "-DBUILD_USD_NET=TRUE -DBUILD_TESTS=FALSE "
    "-DCMAKE_MODULE_PATH=./cmake/modules ".format(usd_python_dir_path, usd_no_python_dir_path, unity_version)
    
    if platform.system() == "Windows":
        cmake_cmd += "-G \"Visual Studio 15 2017 Win64\"" 
        
    build_cmd = "cmake --build build --target install"
          
    p = subprocess.Popen(shlex.split(cmake_cmd))
    p.wait()
    
    p = subprocess.Popen(shlex.split(build_cmd))
    p.wait()