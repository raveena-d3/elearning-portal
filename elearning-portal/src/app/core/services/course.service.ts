
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CourseCreateDTO, CourseResponseDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private api = `${environment.apiUrl}/course`;
  private videoApi = `/api/course`;
  constructor(private http: HttpClient) {}

  getAll() { return this.http.get<CourseResponseDTO[]>(this.api); }
  getById(id: number) { return this.http.get<CourseResponseDTO>(`${this.api}/${id}`); }
  getMyCourses() {
    return this.http.get<CourseResponseDTO[]>(`${this.api}/my-courses`);
}
  create(dto: CourseCreateDTO) { return this.http.post<CourseResponseDTO>(this.api, dto); }
  update(id: number, dto: CourseCreateDTO) { return this.http.put<CourseResponseDTO>(`${this.api}/${id}`, dto); }
  delete(id: number) { return this.http.delete(`${this.api}/${id}`); }
   uploadVideo(courseId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<CourseResponseDTO>(
        `${this.videoApi}/${courseId}/upload-video`, formData
    );
}

getVideoUrl(courseId: number): string {
    return `/api/course/${courseId}/video`;
}

deleteVideo(courseId: number) {
    return this.http.delete(`${this.videoApi}/${courseId}/video`);
}
}