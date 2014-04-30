#
# The MIT License (MIT)
#
# Copyright (c) 2013 Alex Rønne Petersen
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#

CAT ?= cat
CD ?= cd
CHMOD ?= chmod
CP ?= cp
ECHO ?= echo
FSHARPC ?= fsharpc
GENDARME ?= gendarme
MCS ?= mcs
MKDIR ?= mkdir
PKG_CONFIG ?= pkg-config
SED ?= sed
TAR ?= tar
XBUILD ?= xbuild

export MONO_PREFIX ?= /usr
MODE ?= Debug

ifeq ($(MODE), Debug)
	override xb_mode = net_4_0_Debug
	override mono_opt = --debug

	FSHARPC_FLAGS += --debug+
	MCS_FLAGS += -debug
else
	override xb_mode = net_4_0_Release
	override mono_opt =

	FSHARPC_FLAGS += --optimize
	MCS_FLAGS += -optimize
endif

FSHARPC_FLAGS += --nologo --warnaserror
GENDARME_FLAGS += --severity all --confidence all
MCS_FLAGS += -langversion:future -unsafe -warnaserror
XBUILD_FLAGS += /verbosity:quiet /nologo /property:Configuration=$(xb_mode)

FSHARPC_TEST_FLAGS += --debug+ --nologo --warnaserror
MCS_TEST_FLAGS += -debug -langversion:future -unsafe -warnaserror

.PHONY: all check clean clean-check clean-deps gendarme update-deps

override results = \
	LICENSE \
	README \
	sdb.exe \
	sdb.exe.config \
	sdb

all: $(addprefix bin/, $(results))

define TargetType
$(if $(findstring .exe, $(1)),exe,library)
endef

override cs_tests = \
	cwl.exe \
	throw.exe

define CSharpTestTemplate
chk/cs/$(1): chk/cs/$(basename $(1)).cs
	$(MCS) $(MCS_TEST_FLAGS) -out:chk/cs/$(1) -target:$(call TargetType, $(1)) chk/cs/$(basename $(1)).cs
endef

$(foreach tgt, $(cs_tests), $(eval $(call CSharpTestTemplate,$(tgt))))

override fs_tests = \
	print.exe

define FSharpTestTemplate
chk/fs/$(1): chk/fs/$(basename $(1)).fs
	$(FSHARPC) $(FSHARPC_TEST_FLAGS) --out:chk/fs/$(1) --target:$(call TargetType, $(1)) chk/fs/$(basename $(1)).fs
endef

$(foreach tgt, $(fs_tests), $(eval $(call FSharpTestTemplate,$(tgt))))

override tests = \
	$(addprefix chk/cs/, $(cs_tests)) \
	$(addprefix chk/cs/, $(addsuffix .mdb, $(cs_tests))) \
	$(addprefix chk/fs/, $(fs_tests)) \
	$(addprefix chk/fs/, $(addsuffix .mdb, $(fs_tests)))

check: chk/check.exe $(addprefix bin/, $(results)) $(tests)
	$(CD) chk && MONO_PATH=. $(MONO_PREFIX)/bin/mono check.exe

clean:
	$(RM) -r bin

clean-check:
	$(RM) chk/check.exe chk/check.exe.mdb
	$(RM) $(tests)

clean-deps:
	$(CD) dep/debugger-libs && $(XBUILD) $(XBUILD_FLAGS) /target:Clean

gendarme: bin/sdb.exe
	$(GENDARME) $(GENDARME_FLAGS) --log bin/sdb.log $<

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

override srcs = \
	src/Options.cs \
	src/getline.cs \
	src/Commands/AliasCommand.cs \
	src/Commands/ArgumentsCommand.cs \
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
	src/Commands/DoCommand.cs \
	src/Commands/EnvironmentCommand.cs \
	src/Commands/FrameCommand.cs \
	src/Commands/HelpCommand.cs \
	src/Commands/JumpCommand.cs \
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
	src/LibEdit.cs \
	src/Log.cs \
	src/MultiCommand.cs \
	src/Plugins.cs \
	src/Program.cs \
	src/SessionKind.cs \
	src/State.cs \
	src/Utilities.cs

bin/sdb.exe: $(srcs) $(addprefix bin/, $(refs)) mono.snk
	$(MCS) $(MCS_FLAGS) -keyfile:mono.snk -lib:bin -out:bin/sdb.exe -target:exe -r:Mono.Posix $(addprefix -r:, $(refs)) $(srcs)

bin/sdb.exe.config: sdb.exe.config
	$(MKDIR) -p bin
	$(CP) $< $@

bin/sdb: sdb.in
	$(MKDIR) -p bin
	$(SED) s/__MONO_OPTIONS__/$(mono_opt)/ $< > $@
	$(CHMOD) +x $@

bin/LICENSE: LICENSE
	$(MKDIR) -p bin
	$(CP) $< $@

bin/README: README.md
	$(MKDIR) -p bin
	$(CP) $< $@

chk/check.exe: chk/check.fs mono.snk
	$(FSHARPC) $(FSHARPC_FLAGS) --keyfile:mono.snk --out:$@ --target:exe chk/check.fs

sdb.tar.gz: $(addprefix bin/, $(results))
	$(RM) sdb.tar.gz
	$(CD) bin && $(TAR) -zcf ../sdb.tar.gz $(results) $(refs)

update-deps:
	$(ECHO) "/* DO NOT EDIT - OVERWRITTEN BY MAKEFILE */\n" > src/Options.cs
	$(CAT) `$(PKG_CONFIG) --variable=Sources mono-options` >> src/Options.cs
	$(ECHO) "/* DO NOT EDIT - OVERWRITTEN BY MAKEFILE */\n" > src/getline.cs
	$(CAT) `$(PKG_CONFIG) --variable=Sources mono-lineeditor` >> src/getline.cs
