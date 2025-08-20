import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkspaceCreateDialog } from './workspace-create-dialog';

describe('WorkspaceCreateDialog', () => {
  let component: WorkspaceCreateDialog;
  let fixture: ComponentFixture<WorkspaceCreateDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkspaceCreateDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkspaceCreateDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
