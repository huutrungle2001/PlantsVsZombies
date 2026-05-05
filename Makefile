UNITY ?= /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity
PROJECT_PATH := $(CURDIR)

.DEFAULT_GOAL := run

.PHONY: help setup build run clean

help:
	@echo "Targets:"
	@echo "  make setup  - create default scene and build settings"
	@echo "  make build  - build using BuildScript.Build"
	@echo "  make run    - build then launch the game"
	@echo "  make clean  - remove Builds/ output"

setup:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod ProjectSetup.CreateDefaultScene -logFile -

build:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod BuildScript.Build -logFile -

run: 
	@open "Builds/PVZ.app"

clean:
	@rm -rf Builds
