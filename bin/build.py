#!/usr/bin/env python3 -B
from builtins import FileNotFoundError

import logging
import os
import platform
import shlex
import subprocess
import argparse
import zipfile

STEVEDORE_REPO = "https://artifactory.prd.it.unity3d.com/artifactory/stevedore-testing"
USD_BINARIES = {"20.08": {"Windows": "usd-win-python36-x86_64/v22.03_ad25f6b6e52e2b9496e36ba04a1136742e90ea975ffae54fc6206751bfed9402.zip" ,
                          "Linux": "usd-linux-python36-x86_64/v20.08_a47ac54028df326afe4f871a1cd2b01aa3eab2b0819cc56abe1e90883d2ef97b.zip",
                          "Darwin": "usd-mac-python36-x86_64/v22.03_1e727466bd4747df371aa7ec52376615629d125036b405ff3d606d299b4e900f.zip"}}
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

    output_path = os.path.join(output_dir, usd_binaries_dirname(usd_version, python_version))
    try:
        artifactory_usd_archive = USD_BINARIES[usd_version][platform.system()]
    except KeyError as ex:
        logging.error("USD v{} binaries are not uploaded on Stevedore. Available versions are: [{}]".format(usd_version,
                                                                                                            ",".join(USD_BINARIES.keys())))
        raise ex

    usd_archive_path = os.path.join(output_dir, artifactory_usd_archive.split('/')[-1])

    if not os.path.exists(output_path):
        # Download usd archive from artifactory/stevedore
        if not os.path.exists(usd_archive_path):
            logging.info("Downloading USD v{0} for python {1} to {2} ...".format(usd_version, python_version, output_dir))
            p = subprocess.Popen(shlex.split('wget --no-check-certificate -P "{0}" {1}/{2}'.format(output_dir, STEVEDORE_REPO,
                                                                            artifactory_usd_archive)))
            p.wait()

        # Extract archive
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

    parser.add_argument("--usd_version", dest="usd_version", default="20.08",
                        help="Version of the USD library (ex: 20.08).")
    parser.add_argument("--unity_version", dest="unity_version", default="2019.4",
                        help="Version of Unity (ex: 2019.4). "
                             "Used to find the C# compiler when building the usdnet component")
    parser.add_argument("--library_path", dest="library_path", default="./artifacts",
                        help="Path to the root of the usd install folders (ex: usd-v20.08 and usd-v20.08_no_python)")
    parser.add_argument("--download", dest="download_usd_binaries", action="store_true", default=False,
                        help="Download USD binaries from Unity's Stevedore internal repository. "
                        "Refer to BUILDING.md for command used to build the libraries")
    parser.add_argument("--clean", dest="cmake_target", action="store_const", const="clean", default="install",
                        help="Call cmake with the clean target.")
    parser.add_argument("--component", dest="component", choices=["usdcs", "usdnet", "tests"], default="usdcs")
    parser.add_argument("-v", "--verbose", action="store_true", default=False,
                        help="Set the CMake verbose flag.")
    parser.add_argument("--use_custom_mono", action="store_true", default=False,
                        help="Use a custom mono version. Default is to use the mono compiler provided by Unity.")

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
        try:
            (usd_python_dir_path, usd_no_python_dir_path) = download_usd_binaries(args.usd_version, PYTHON_VERSION,
                                                                                  library_path)
        except KeyError:
            exit()
    else:
        usd_python_dir_path = os.path.join(library_path, usd_binaries_dirname(args.usd_version, PYTHON_VERSION),
                                           usd_python_dirname(args.usd_version, PYTHON_VERSION))
        usd_no_python_dir_path = os.path.join(library_path, usd_binaries_dirname(args.usd_version, PYTHON_VERSION),
                                              usd_no_python_dirname(args.usd_version))

    if not os.path.exists(usd_python_dir_path):
        raise FileNotFoundError(usd_python_dir_path)
    if not os.path.exists(usd_no_python_dir_path):
        raise FileNotFoundError(usd_no_python_dir_path)

    build_dir = "./build"
    if args.component == "usdcs":
        build_dir += "_usdcs"
    elif args.component == "usdnet":
        build_dir += "_usdnet"
    elif args.component == "tests":
        build_dir += "_tests"
    if not os.path.exists(build_dir):
        os.mkdir(build_dir)

    cmake_cmd = " ".join(["cmake -S . -B {} ",
                          '-DPXR_USD_LOCATION="{}" '
                          '-DPXR_USD_LOCATION_PYTHON_BUILD="{}" '
                          "-DUNITY_VERSION={} ",
                          "-DBUILD_USDCS={} ",
                          "-DBUILD_USD_NET={} ",
                          "-DBUILD_TESTS={} ",
                          "-DCMAKE_BUILD_TYPE=RelWithDebInfo",
                          "-DCMAKE_MODULE_PATH=./cmake/modules",
                          "-DUSE_CUSTOM_MONO={} "]).format(build_dir, usd_no_python_dir_path,
                                                                          usd_python_dir_path,
                                                                          args.unity_version,
                                                                          args.component == "usdcs",
                                                                          args.component == "usdnet",
                                                                          args.component == "tests",
                                                                          args.use_custom_mono)

    if args.verbose:
        cmake_cmd += "-DCMAKE_VERBOSE_MAKEFILE:BOOL=ON "

    if platform.system() == "Windows":
        cmake_cmd += "-G \"Visual Studio 16 2019\""

    logging.info("="*80)
    logging.info("Running CMake:\n{}\n\n".format(cmake_cmd))
    p = subprocess.Popen(shlex.split(cmake_cmd))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=cmake_cmd))
    logging.info("\n\n")

    build_cmd = "cmake --build {} --config RelWithDebInfo --target {}".format(build_dir, args.cmake_target)
    logging.info("Running CMake build:\n{}\n\n".format(build_cmd))
    p = subprocess.Popen(shlex.split(build_cmd))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=build_cmd))
