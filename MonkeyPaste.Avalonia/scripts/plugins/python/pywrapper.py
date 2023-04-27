import sys
import importlib
import importlib.util

print("arg1:")
print(sys.argv[1])
spec = importlib.util.spec_from_file_location("module.plugin", sys.argv[1])
foo = importlib.util.module_from_spec(spec)
sys.modules["module.plugin"] = foo
spec.loader.exec_module(foo)

spec2 = importlib.util.spec_from_file_location("module.anlyzer", "C:\\Users\\tkefauver\\Source\\Repos\\MonkeyPaste\\Plugins\\Declarative\\PyTest\\analyzer.py")
foo2 = importlib.util.module_from_spec(spec2)
sys.modules["module.analyzer"] = foo2
foo.analyze()

#wrapped_analyzer = importlib.import_module(sys.argv[1])
#wrapped_analyzer.analyze()
