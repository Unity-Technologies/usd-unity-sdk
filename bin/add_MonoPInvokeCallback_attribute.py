import os

USD_CS_PINVOKE_FILEPATH = "../cmake/generated/UsdCsPINVOKE.cs"
USD_CS_PINVOKE_OUT_FILEPATH = "../cmake/generated/UsdCsPINVOKE.cs.out"
DECORATORS = ["// ATTRIBUTE AUTOMATICALLY ADDED.",
              "[MonoPInvokeCallback]"]

METHODS_TO_DECORATE = ["SetPendingApplicationException(",
                      "SetPendingArithmeticException(",
                      "SetPendingDivideByZeroException(",
                      "SetPendingIndexOutOfRangeException(",
                      "SetPendingInvalidCastException(",
                      "SetPendingInvalidOperationException(",
                      "SetPendingIOException(",
                      "SetPendingNullReferenceException(",
                      "SetPendingOutOfMemoryException(",
                      "SetPendingOverflowException(",
                      "SetPendingSystemException(",
                      "SetPendingArgumentException(",
                      "SetPendingArgumentNullException(",
                      "SetPendingArgumentOutOfRangeException(",
                      "EntryPoint=\"SWIGRegisterExceptionCallbacks_UsdCs",
                      "EntryPoint=\"SWIGRegisterExceptionArgumentCallbacks_UsdCs",
                      "CreateString("]

def decorate(line):
    print("decorating: {0}".format(line))
    leading_spaces = len(line) - len(line.lstrip(' '))
    decorated = ["{0}{1}".format(" "*leading_spaces, d) for d in DECORATORS]
    decorated.append(line)
    return "\n".join(decorated)

def main():
    print("Adding MonoPInvokeCallback attribute to the appropriate methods ...\n")

    cwd = os.path.abspath(os.path.dirname(__file__))
    filein_path = os.path.join(cwd, USD_CS_PINVOKE_FILEPATH)
    fileout_path = os.path.join(cwd, USD_CS_PINVOKE_OUT_FILEPATH)
    with open(filein_path, "r") as filein, open(fileout_path, "w") as fileout:
        for line in filein:
            if any(methods in line for methods in METHODS_TO_DECORATE):
                fileout.write(decorate(line))
            else:
                fileout.write(line)
    os.replace(fileout_path, filein_path)

if __name__ == "__main__":
    main()
