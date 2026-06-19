import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { LessonDetailPage } from './lesson-detail';
import { KeycloakService } from '../../auth/keycloak.service';
import { LessonDetail, LessonService } from '../../services/lesson.service';

const lesson: LessonDetail = {
  id: 1,
  courseId: 10,
  order: 1,
  title: 'Intro lesson',
  durationMinutes: 45,
  requirementIcons: [],
  courseName: 'Course',
  suggestions: [],
  attendances: [],
};

describe('LessonDetailPage', () => {
  it('renders markdown through Angular sanitization instead of trusting uploaded HTML', async () => {
    const lessonService = {
      getLessonById: vi.fn().mockReturnValue(of(lesson)),
      getLessonFiles: vi.fn().mockReturnValue(of([{ uuid: 'file-1', name: 'lesson.md', size: 100 }])),
      downloadMaterialAsText: vi.fn().mockResolvedValue(
        '# Safe heading\n\n<script>window.xss = true;</script><img src="x" onerror="window.xss = true">'
      ),
    };

    await TestBed.configureTestingModule({
      imports: [LessonDetailPage],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { params: of({ id: '1' }) } },
        { provide: LessonService, useValue: lessonService },
        { provide: KeycloakService, useValue: { hasRole: vi.fn().mockReturnValue(false) } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(LessonDetailPage);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const markdownContent = fixture.nativeElement.querySelector('.markdown-content') as HTMLElement;

    expect(markdownContent.querySelector('h1')?.textContent).toBe('Safe heading');
    expect(markdownContent.querySelector('script')).toBeNull();
    expect(markdownContent.querySelector('img')?.hasAttribute('onerror')).toBe(false);
  });
});
