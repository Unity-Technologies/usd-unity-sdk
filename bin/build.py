#!/usr/bin/python -B
import logging
import os
import platform
import sys
import shlex
import subprocess
import argparse
import zipfile

STEVEDORE_REPO = "https://artifactory.internal.unity3d.com/stevedore-testing"
USD_BINARIES = {"20.08": {"Windows": "usd-win-python36-x86_64/v20.08_1df762cff26f05e8c53edbc217cf0fa1210be67d75ff845316124867e33c6869.zip" ,
                          "Linux": "usd-linux-python36-x86_64/v20.08_a47ac54028df326afe4f871a1cd2b01aa3eab2b0819cc56abe1e90883d2ef97b.zip",
                          "Darwin": "usd-mac-python36-x86_64/v20.08_e2df4db2fe24542b50f21e9ae2df45768f533c5ab03956f4d8dff64e773ed065.zip"}}
PYTHON_VERSION = "36"

def usd_python_dirname(usd_version, python_version):
    return "usd-v{0}".format(usd_version)

def usd_no_python_dirname(usd_version):
    return "usd-v{0}_no_python".format(usd_version)

def usd_binaries_dirname(usd_version, python_version):
    return "usd-v{0}-python{1}".format(usd_version, python_version)

def download_usd_binaries(usd_version, python_version=PYTHON_VERSION, output_dir=""):
    output_dir = os.path.abspath(output_dir)
    if not os.path.exists(output_dir):
        logging.error("Target path doesn't exist: {0}".format(output_dir))
        return None, None

    # Download usd archive from artifactory/stevedore
    artifactory_usd_archive = USD_BINARIES[usd_version][platform.system()]
    usd_archive_path = os.path.join(output_dir, artifactory_usd_archive.split('/')[-1])
    if(not os.path.exists(usd_archive_path)):
        logging.info("Downloading USD v{0} for python {1} to {2} ...".format(usd_version, python_version, output_dir))
        p = subprocess.Popen(shlex.split('wget -P "{0}" {1}/{2}'.format(output_dir, STEVEDORE_REPO, artifactory_usd_archive)))
        p.wait()

    # Extract archive
    output_path = os.path.join(output_dir, usd_binaries_dirname(usd_version, python_version))
    logging.info("Extracting to {} ...\n".format(output_path))
    with zipfile.ZipFile(usd_archive_path, 'r') as usd_zip:
        usd_zip.extractall(output_path)

    usd_python_path = os.path.join(output_path, usd_python_dirname(usd_version, python_version))
    usd_no_python_path = os.path.join(output_path, usd_no_python_dirname(usd_version))
    return usd_python_path, usd_no_python_path

if __name__ == "__main__":
    logging.basicConfig()
    logging.getLogger().setLevel(logging.INFO)

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

    # Some versions of usd are tagged with a leading v, so strip it out
    if args.usd_version.startswith("v"):
        args.usd_version = args.usd_version[1:]

    # Create the install folder
    library_path = os.path.abspath(args.library_path)
    if not os.path.exists(library_path):
        os.makedirs(library_path)

    # Download usd binaries from stevedore
    if args.download_usd_binaries:
        (usd_python_dir_path, usd_no_python_dir_path) = download_usd_binaries(args.usd_version, PYTHON_VERSION, library_path)
    else:
        usd_python_dir_path = os.path.join(library_path, usd_binaries_dirname(args.usd_version, PYTHON_VERSION), usd_python_dirname(args.usd_version, PYTHON_VERSION))
        usd_no_python_dir_path = os.path.join(library_path, usd_binaries_dirname(args.usd_version, PYTHON_VERSION), usd_no_python_dirname(args.usd_version))

    if not os.path.exists(usd_python_dir_path):
        raise FileNotFoundError(usd_python_dir_path)
    if not os.path.exists(usd_no_python_dir_path):
        raise FileNotFoundError(usd_no_python_dir_path)


    if not os.path.exists("./build"):
        os.mkdir("build")

    cmake_cmd = " ".join(["cmake -S . -B build ",
                          "-DPXR_USD_LOCATION={} "
                          "-DPXR_USD_LOCATION_PYTHON_BUILD={} "
                          "-DUNITY_VERSION={} ",
                          "-DBUILD_USD_NET=TRUE -DBUILD_TESTS=FALSE ",
                          "-DCMAKE_BUILD_TYPE=RelWithDebInfo",
                          "-DCMAKE_MODULE_PATH=./cmake/modules "]).format(usd_no_python_dir_path, usd_python_dir_path, args.unity_version)

    if platform.system() == "Windows":
        cmake_cmd += "-G \"Visual Studio 15 2017 Win64\""

    logging.info("="*80)
    logging.info("Running CMake:\n{}\n\n".format(cmake_cmd))
    p = subprocess.Popen(shlex.split(cmake_cmd))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=cmake_cmd))
    logging.info("\n\n")

    build_cmd = "cmake --build build --config RelWithDebInfo --target install"
    logging.info("Running CMake build:\n{}\n\n".format(build_cmd))
    p = subprocess.Popen(shlex.split(build_cmd))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=cmake_cmd))
