FROM ubuntu:18.04

RUN apt-get -qq update && apt-get -y install python3-pip
RUN pip3 install --upgrade pip
RUN apt-get install python3-setuptools;

RUN apt-get -y install git 
RUN git clone --depth 1 --branch v20.08 https://github.com/PixarAnimationStudios/USD 

WORKDIR USD

RUN apt-get -qq update && apt-get -y install python-setuptools libglew-dev libxrandr-dev libxcursor-dev libxinerama-dev libxi-dev zlib1g-dev libopenexr22
RUN pip3 install cmake --upgrade
RUN pip3 install PyOpenGL 

RUN python3 build_scripts/build_usd.py --build-monolithic --alembic --no-python --no-imaging --no-examples --no-tutorials ../artifacts/usd-v20.08_no_python






