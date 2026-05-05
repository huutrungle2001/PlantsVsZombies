UNITY ?= /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity
PROJECT_PATH := $(CURDIR)

.DEFAULT_GOAL := run

.PHONY: help setup build run clean

help:
	@echo "Targets:"
	@echo "  make setup  - one-time bootstrap: scene, board, prefabs, GameManager wiring"
	@echo "  make build  - compile and build the macOS app"
	@echo "  make run    - launch the built app"
	@echo "  make clean  - remove Builds/ output"

setup:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod ProjectSetup.FullSetup -logFile -

build:
	$(UNITY) -batchmode -quit -nographics -projectPath "$(PROJECT_PATH)" -executeMethod BuildScript.Build -logFile -

run:
	@open "Builds/PVZ.app"

clean:
	@rm -rf Builds
