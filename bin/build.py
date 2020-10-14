#!/usr/bin/python -B
import gdown
import logging
import os
import platform
import sys
import shlex
import subprocess
import argparse 

USD_BINARIES_PYTHON = {"20.08": {"Windows": "https://drive.google.com/uc?id=1GDTMhT3GK9k0mAtJIhtB7lZ5PxW9SRVJ" ,
                                 "Linux": "",
                                 "Darwin":""}}
USD_BINARIES_NO_PYTHON = {"20.08": {"Windows": "https://drive.google.com/uc?id=1uE_a9t9fHv-ldTYZEIhnsuhiLuo2pw-O" ,
                                 "Linux": "",
                                 "Darwin":""}}

def usd_python_dirname(usd_version):
    return "usd-v{0}".format(usd_version)
    
def usd_no_python_dirname(usd_version):
    return "usd-v{0}_no_python".format(usd_version)
    
def download_usd_python_binaries(usd_version, output_dir=""):
    if not os.path.exists(output_dir):
        logging.error("Target path doesn't exist: {0}".format(output_dir))
        return
        
    output_path = os.path.join(output_dir, usd_python_dirname(usd_version))
    output_path = os.path.normpath(output_path)
    if os.path.exists(output_path):
        logging.info("USD v{0} python build found.".format(usd_version))
        return output_path
        
    logging.info("Downloading USD v{0} python build to {1} ...".format(usd_version, output_path))
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
        logging.info("USD v{0} no python build found.".format(usd_version))
        return output_path
        
    logging.info("Downloading USD v{0} no python build to {1} ...".format(usd_version, output_path))
    gdown.download(USD_BINARIES_NO_PYTHON[usd_version][platform.system()], output_path+".zip")
    # TODO: unzip
    return output_path
    
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Build script for the USD C# bindings.")
    
    parser.add_argument("usd_version", type=str, 
                        help="Version of the USD library (ex: 20.08).")
    parser.add_argument("library_path", type=str, 
                        help="Path to the usd install directory")                        
    parser.add_argument("unity_version", type=str, 
                        help="Version of Unity (ex: 2019.4)")
    parser.add_argument("--download", dest="download_usd_binaries", action="store_true", default=False,
                        help="Download USD binaries from Unity's Stevedore internal repository. "
                        "Refer to BUILDING.md for command used to build the libraries")

    args = parser.parse_args()                        
    
    if args.download_usd_binaries:
        usd_python_dir_path = download_usd_python_binaries(args.usd_version, args.library_path)
        usd_no_python_dir_path = download_usd_no_python_binaries(args.usd_version, args.library_path)
    else:
        usd_python_dir_path = os.path.join(args.library_path, usd_python_dirname(usd_version))
        usd_nopython_dir_path = os.path.join(args.library_path, usd_no_python_dirname(usd_version))
        
    if not os.path.exists("./build"):
        os.path.make_dir("build")
        
    cmake_cmd = "cmake -S . -B build "
    "-DPXR_USD_LOCATION={0} " 
    "-DPXR_USD_LOCATION_PYTHON_BUILT={1} "
    "-DUNITY_VERSION={2} "
    "-DBUILD_USD_NET=TRUE -DBUILD_TESTS=FALSE "
    "-DCMAKE_MODULE_PATH=./cmake/modules ".format(usd_python_dir_path, usd_no_python_dir_path, args.unity_version)
    
    if platform.system() == "Windows":
        cmake_cmd += "-G \"Visual Studio 15 2017 Win64\"" 
        
    build_cmd = "cmake --build build --target install"
          
    p = subprocess.Popen(shlex.split(cmake_cmd))
    p.wait()
    
    p = subprocess.Popen(shlex.split(build_cmd))
    p.wait()