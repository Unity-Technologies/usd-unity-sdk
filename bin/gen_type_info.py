#!/usr/bin/python3 -B
import logging
import subprocess
import os
import sys

# TODO: add proper argument handling
if __name__ == "__main__":
    logging.basicConfig()
    logging.getLogger().setLevel(logging.INFO)

    # Add usd to the environment
    usd_location = sys.argv[1]
    out_path = sys.argv[2]
    new_env = os.environ.copy()
    new_env["PATH"] = os.pathsep.join([os.path.join(usd_location, "bin"),
                                           os.path.join(usd_location, "lib"),
                                           new_env.get("PATH", "")])
    new_env["LD_LIBRARY_PATH"] = os.pathsep.join([os.path.join(usd_location, "lib"),
                                                  new_env.get("LD_LIBRARY_PATH", "")])
    usd_python_path = os.path.join(usd_location, "lib", "python")
    new_env["PYTHONPATH"] = os.pathsep.join([usd_python_path, new_env.get("PYTHONPATH", "")])

    # Spawn a python process with the new environment
    cmd = ["python3", "src/Swig/scripts/gen.py", out_path]
    p = subprocess.Popen(cmd, env=new_env, cwd=os.path.join(os.getcwd(), '..'))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=cmd))
