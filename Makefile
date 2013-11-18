CD ?= cd
CHMOD ?= chmod
CP ?= cp
FSHARPI ?= fsharpi
GENDARME ?= gendarme
MCS ?= mcs
MKDIR ?= mkdir
PKG_CONFIG ?= pkg-config
SED ?= sed
TAR ?= tar
XBUILD ?= xbuild

MODE ?= Debug

ifeq ($(MODE), Debug)
	override xb_mode = net_4_0_Debug
	override mono_opt = --debug

	MCS_FLAGS += -debug
else
	override xb_mode = net_4_0_Release
	override mono_opt =

	MCS_FLAGS += -optimize
endif

MCS_FLAGS += -langversion:future -unsafe -warnaserror
XBUILD_FLAGS += /verbosity:quiet /nologo /property:Configuration=$(xb_mode)
GENDARME_FLAGS += --severity all --confidence all
FSHARPI_FLAGS += --exec

.PHONY: all clean gendarme check

override results = \
	sdb.exe \
	sdb.exe.config \
	sdb \
	COPYING \
	README

all: $(addprefix bin/, $(results))

clean:
	$(RM) -r bin
	$(CD) dep/debugger-libs && $(XBUILD) $(XBUILD_FLAGS) /target:Clean

gendarme: bin/sdb.exe
	$(GENDARME) $(GENDARME_FLAGS) --log bin/sdb.log $<

check: $(addprefix bin/, $(results))
	$(CD) chk && $(FSHARPI) $(FSHARPI_FLAGS) check.fsx

override refs = \
	ICSharpCode.NRefactory.dll \
	ICSharpCode.NRefactory.CSharp.dll \
	Mono.Cecil.dll \
	Mono.Cecil.Mdb.dll \
	Mono.Debugger.Soft.dll \
	Mono.Debugging.dll \
	Mono.Debugging.Soft.dll

$(addprefix bin/, $(refs)):
	$(CD) dep/debugger-libs && $(XBUILD) $(XBUILD_FLAGS) debugger-libs.sln
	$(MKDIR) -p bin
	$(CP) dep/nrefactory/bin/Debug/ICSharpCode.NRefactory.dll \
		bin/ICSharpCode.NRefactory.dll
	$(CP) dep/nrefactory/bin/Debug/ICSharpCode.NRefactory.CSharp.dll \
		bin/ICSharpCode.NRefactory.CSharp.dll
	$(CP) dep/cecil/bin/$(xb_mode)/Mono.Cecil.dll \
		bin/Mono.Cecil.dll
	$(CP) dep/cecil/bin/$(xb_mode)/Mono.Cecil.Mdb.dll \
		bin/Mono.Cecil.Mdb.dll
	$(CP) dep/debugger-libs/Mono.Debugger.Soft/bin/Debug/Mono.Debugger.Soft.dll \
		bin/Mono.Debugger.Soft.dll
	$(CP) dep/debugger-libs/Mono.Debugging/bin/Debug/Mono.Debugging.dll \
		bin/Mono.Debugging.dll
	$(CP) dep/debugger-libs/Mono.Debugging.Soft/bin/Debug/Mono.Debugging.Soft.dll \
		bin/Mono.Debugging.Soft.dll

dep/Options.cs:
	$(CP) `$(PKG_CONFIG) --variable=Sources mono-options` $@

dep/getline.cs:
	$(CP) `$(PKG_CONFIG) --variable=Sources mono-lineeditor` $@

override srcs = \
	dep/Options.cs \
	dep/getline.cs \
	src/Commands/AttachCommand.cs \
	src/Commands/BacktraceCommand.cs \
	src/Commands/BreakpointCommand.cs \
	src/Commands/CatchpointCommand.cs \
	src/Commands/ConfigCommand.cs \
	src/Commands/ConnectCommand.cs \
	src/Commands/ContinueCommand.cs \
	src/Commands/DatabaseCommand.cs \
	src/Commands/DecompileCommand.cs \
	src/Commands/DirectoryCommand.cs \
	src/Commands/DisassembleCommand.cs \
	src/Commands/EnvironmentCommand.cs \
	src/Commands/FrameCommand.cs \
	src/Commands/HelpCommand.cs \
	src/Commands/KillCommand.cs \
	src/Commands/ListenCommand.cs \
	src/Commands/PluginCommand.cs \
	src/Commands/PrintCommand.cs \
	src/Commands/QuitCommand.cs \
	src/Commands/ResetCommand.cs \
	src/Commands/RootCommand.cs \
	src/Commands/RunCommand.cs \
	src/Commands/SourceCommand.cs \
	src/Commands/StepCommand.cs \
	src/Commands/ThreadCommand.cs \
	src/Commands/WatchCommand.cs \
	src/AssemblyInfo.cs \
	src/Color.cs \
	src/Command.cs \
	src/CommandAttribute.cs \
	src/CommandLine.cs \
	src/Configuration.cs \
	src/CustomLogger.cs \
	src/Debugger.cs \
	src/LibC.cs \
	src/LibReadLine.cs \
	src/Log.cs \
	src/MultiCommand.cs \
	src/Plugins.cs \
	src/Program.cs \
	src/SessionKind.cs \
	src/State.cs \
	src/Utilities.cs

bin/sdb.exe: $(srcs) $(addprefix bin/, $(refs)) mono.snk
	$(MCS) $(MCS_FLAGS) -keyfile:mono.snk -lib:bin -out:bin/sdb.exe -target:exe $(addprefix -r:, $(refs)) $(srcs)

bin/sdb.exe.config: sdb.exe.config
	$(MKDIR) -p bin
	$(CP) $< $@

bin/sdb: sdb.in
	$(MKDIR) -p bin
	$(SED) s/__MONO_OPTIONS__/$(mono_opt)/ $< > $@
	$(CHMOD) +x $@

bin/COPYING: COPYING
	$(MKDIR) -p bin
	$(CP) $< $@

bin/README: README.md
	$(MKDIR) -p bin
	$(CP) $< $@

sdb.tar.gz: $(addprefix bin/, $(results))
	$(RM) sdb.tar.gz
	$(CD) bin && $(TAR) -zcf ../sdb.tar.gz $(results) $(refs)
