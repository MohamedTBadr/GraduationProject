import { Component, Input, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface UploadedImage {
  file?: File;
  previewUrl: string | ArrayBuffer | null;
  progress?: number;
  status?: 'pending' | 'uploading' | 'done' | 'error';
}

@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-upload.component.html',
  styleUrls: ['./image-upload.component.scss']
})
export class ImageUploadComponent {
  @Input() multiple = true;
  @Input() maxFiles = 5;
  @Input() maxSizeMB = 5;
  @Input() acceptFormats = ['image/jpeg', 'image/png', 'image/webp'];
  
  /** Existing server image URLs (e.g. when editing a service). */
  @Input() set initialImages(urls: string[] | null | undefined) {
    this.images = (urls ?? [])
      .filter(url => !!url)
      .map(url => ({
        previewUrl: url,
        status: 'done' as const
      }));
    this.emitChanges();
  }

  @Output() imagesChanged = new EventEmitter<UploadedImage[]>();

  images: UploadedImage[] = [];
  dragOver = false;
  errorMsg = '';

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  onFileDropped(event: DragEvent) {
    event.preventDefault();
    this.dragOver = false;
    this.errorMsg = '';
    
    if (event.dataTransfer?.files) {
      this.handleFiles(event.dataTransfer.files);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.dragOver = false;
  }

  onFileSelected(event: any) {
    const files = event.target.files;
    this.handleFiles(files);
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  handleFiles(files: FileList) {
    this.errorMsg = '';
    const newFiles = Array.from(files);

    if (!this.multiple && newFiles.length > 0) {
      this.processFile(newFiles[0], true);
    } else {
      if (this.images.length + newFiles.length > this.maxFiles) {
        this.errorMsg = `You can only upload up to ${this.maxFiles} images.`;
        return;
      }
      newFiles.forEach(file => this.processFile(file, false));
    }
  }

  processFile(file: File, clearExisting: boolean) {
    if (!this.acceptFormats.includes(file.type)) {
      this.errorMsg = `File ${file.name} is not a supported format. Max allowed formats: JPG, PNG, WebP.`;
      return;
    }
    
    const sizeMB = file.size / (1024 * 1024);
    if (sizeMB > this.maxSizeMB) {
      this.errorMsg = `File ${file.name} exceeds the maximum size of ${this.maxSizeMB}MB.`;
      return;
    }

    if (clearExisting) {
      this.images = [];
    }

    const newImg: UploadedImage = {
      file,
      previewUrl: null,
      status: 'pending'
    };

    this.images.push(newImg);
    // Emit immediately so the parent has the File before form submit.
    this.emitChanges();

    const reader = new FileReader();
    reader.onload = (e) => {
      newImg.previewUrl = e.target?.result || null;
      newImg.status = 'done';
      this.emitChanges();
    };
    reader.readAsDataURL(file);
  }

  removeImage(index: number) {
    this.images.splice(index, 1);
    this.emitChanges();
  }

  emitChanges() {
    this.imagesChanged.emit(
      this.images.filter(img => !!img.file || !!img.previewUrl)
    );
  }
}
