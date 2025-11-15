import 'zone.js/testing';
import { getTestBed, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';

getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
);

const defaultImports = [HttpClientTestingModule, RouterTestingModule, NoopAnimationsModule];
const matDialogRefMock = {
  close: jasmine.createSpy('close'),
  afterClosed: () => of(true),
};

const originalConfigureTestingModule = TestBed.configureTestingModule;

TestBed.configureTestingModule = function (moduleDef) {
  moduleDef = moduleDef ?? {};
  moduleDef.imports = [...defaultImports, ...(moduleDef.imports ?? [])];
  moduleDef.providers = [
    { provide: MatDialogRef, useValue: matDialogRefMock },
    { provide: MAT_DIALOG_DATA, useValue: {} },
    ...(moduleDef.providers ?? []),
  ];
  return originalConfigureTestingModule.call(this, moduleDef);
};
