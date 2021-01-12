#!/usr/bin/python3 -B
import logging
import subprocess
import os
import sys

if __name__ == "__main__":
    logging.basicConfig()
    logging.getLogger().setLevel(logging.INFO)

    # Add usd to the environment
    usd_location = sys.argv[1]
    new_env = os.environ.copy()
    new_env["PATH"] = "{0}:{1}:{2}".format(os.path.join(usd_location, "bin"),
                                           os.path.join(usd_location, "lib"),
                                           new_env["PATH"])
    new_env["PYTHONPATH"] = "{0}:{1}".format(os.path.join(usd_location, "lib", "python"),
                                             new_env["PYTHONPATH"])
    logging.getLogger().error(new_env['PATH'])

    # Spawn a python process with the new environment
    cmd = ["python3", "src/Swig/scripts/gen.py"]
    p = subprocess.Popen(cmd, env=new_env, cwd=os.path.join(os.getcwd(), '..'))
    p.wait()
    if p.returncode != 0:
        raise RuntimeError("Failed to run '{cmd}'\n.".format(cmd=cmd))
