/**
 * Registration: draft auto-save (valid fields only), file upload to uploads/draft/.
 * Draft and files expire after 30 min if user never submits.
 */
(function () {
    const IMAGE_EXT = ['.jpeg', '.jpg', '.png'];
    const VIDEO_EXT = ['.mp4', '.mov', '.mkv', '.avi', '.wmv'];
    const IMAGE_MAX_MB = 5;
    const VIDEO_MAX_MB = 100;
    const DEBOUNCE_MS = 500;
    const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function getExt(name) {
        const i = name.lastIndexOf('.');
        return i >= 0 ? name.slice(i).toLowerCase() : '';
    }

    function validateDraftField(field, value) {
        var v = (value || '').trim();
        switch (field) {
            case 'FullName':
                return v.length <= 150;
            case 'DateOfBirth':
                if (!v) return true;
                var d = new Date(v);
                if (isNaN(d.getTime())) return false;
                return d <= new Date();
            case 'HeightCm':
                if (!v) return true;
                var h = parseFloat(v);
                return !isNaN(h) && h >= 0 && h <= 300;
            case 'Gender':
                return v && v !== '0' && v !== 'Unknown';
            case 'MobileNumber':
                return v.length <= 20;
            case 'Email':
                if (!v) return true;
                return v.length <= 256 && EMAIL_RE.test(v);
            default:
                return false;
        }
    }

    function validateFormat(file, isImage) {
        const ext = getExt(file.name);
        const allowed = isImage ? IMAGE_EXT : VIDEO_EXT;
        const maxMb = isImage ? IMAGE_MAX_MB : VIDEO_MAX_MB;
        const allowedList = isImage ? 'JPEG, JPG, PNG' : 'MP4, MOV, MKV, AVI, WMV';
        if (!allowed.includes(ext))
            return { ok: false, msg: (ext || 'Unknown') + ' format is not allowed. Use ' + allowedList + ' only.' };
        if (file.size > maxMb * 1024 * 1024)
            return { ok: false, msg: 'File must be at most ' + maxMb + ' MB.' };
        return { ok: true };
    }

    function uploadWithProgress(file, type, progressBar, progressFill, errorSpan, uploadedSpan, token, input, draftId, onSuccess) {
        if (errorSpan) errorSpan.textContent = '';
        if (uploadedSpan) uploadedSpan.textContent = '';
        progressBar.classList.remove('d-none');
        progressFill.style.width = '0%';

        const formData = new FormData();
        formData.append('type', type);
        formData.append('file', file);
        formData.append('draftId', draftId);
        formData.append('__RequestVerificationToken', token);

        const xhr = new XMLHttpRequest();
        xhr.upload.addEventListener('progress', function (e) {
            if (e.lengthComputable)
                progressFill.style.width = Math.round((e.loaded / e.total) * 100) + '%';
        });
        xhr.addEventListener('load', function () {
            progressBar.classList.add('d-none');
            try {
                const json = JSON.parse(xhr.responseText);
                if (json.success && json.path) {
                    if (uploadedSpan) uploadedSpan.textContent = 'File uploaded';
                    onSuccess && onSuccess(json.path);
                } else {
                    if (errorSpan) errorSpan.textContent = json.error || 'Upload failed';
                    if (typeof toastr !== 'undefined') toastr.error(json.error || 'Upload failed');
                }
            } catch {
                if (errorSpan) errorSpan.textContent = 'Upload failed';
                if (typeof toastr !== 'undefined') toastr.error('Upload failed');
            }
        });
        xhr.addEventListener('error', function () {
            progressBar.classList.add('d-none');
            if (errorSpan) errorSpan.textContent = 'Network error';
            if (typeof toastr !== 'undefined') toastr.error('Network error');
        });
        xhr.open('POST', '/Account/UploadTempFile');
        xhr.send(formData);
    }

    function updateDraftField(draftId, field, value, token) {
        const fd = new FormData();
        fd.append('draftId', draftId);
        fd.append('field', field);
        fd.append('value', value || '');
        fd.append('__RequestVerificationToken', token);
        fetch('/Account/UpdateDraft', { method: 'POST', body: fd }).catch(function () { });
    }

    function debounce(fn, ms) {
        var t;
        return function () {
            clearTimeout(t);
            t = setTimeout(fn, ms);
        };
    }

    function initRegistrationUpload() {
        const form = document.getElementById('registerForm');
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : '';
        const draftIdEl = document.getElementById('reg_DraftId');
        const imgInput = document.getElementById('reg_ProfileImage');
        const vidInput = document.getElementById('reg_ProfileVideo');

        if (!form || !draftIdEl) return;

        function getDraftId() { return draftIdEl.value; }

        function ensureDraft(cb) {
            var id = getDraftId();
            if (id) { cb && cb(); return; }
            const fd = new FormData();
            fd.append('__RequestVerificationToken', token);
            fetch('/Account/CreateDraft', { method: 'POST', body: fd })
                .then(function (r) { return r.json(); })
                .then(function (json) {
                    if (json.draftId) {
                        draftIdEl.value = json.draftId;
                        cb && cb();
                    }
                })
                .catch(function () { });
        }

        var saveField = debounce(function () {
            var draftId = getDraftId();
            if (!draftId) return;
            var field = this.dataset.draftField;
            var val = this.value;
            if (!field) return;
            if (!validateDraftField(field, val)) return;
            updateDraftField(draftId, field, val, token);
        }, DEBOUNCE_MS);

        var fields = [
            { id: 'reg_FullName', field: 'FullName' },
            { id: 'reg_DateOfBirth', field: 'DateOfBirth' },
            { id: 'reg_HeightCm', field: 'HeightCm' },
            { id: 'reg_Gender', field: 'Gender' },
            { id: 'reg_MobileNumber', field: 'MobileNumber' },
            { id: 'reg_Email', field: 'Email' }
        ];
        fields.forEach(function (f) {
            var el = document.getElementById(f.id);
            if (el) {
                el.dataset.draftField = f.field;
                el.addEventListener('input', function () {
                    ensureDraft(function () { saveField.call(el); });
                });
                el.addEventListener('change', function () {
                    ensureDraft(function () { saveField.call(el); });
                });
            }
        });

        if (imgInput && vidInput) {
            const imgError = document.getElementById('reg_ProfileImage_error');
            const vidError = document.getElementById('reg_ProfileVideo_error');
            const imgUploaded = document.getElementById('reg_ProfileImage_uploaded');
            const vidUploaded = document.getElementById('reg_ProfileVideo_uploaded');
            const imgProgress = document.getElementById('reg_ProfileImage_progress');
            const vidProgress = document.getElementById('reg_ProfileVideo_progress');
            const imgProgressFill = imgProgress ? imgProgress.querySelector('.progress-bar') : null;
            const vidProgressFill = vidProgress ? vidProgress.querySelector('.progress-bar') : null;

            function setupInput(input, isImage, errorSpan, progressBar, progressFill, uploadedSpan) {
                input.addEventListener('change', function () {
                    if (uploadedSpan) uploadedSpan.textContent = '';
                    if (errorSpan) errorSpan.textContent = '';
                    const file = input.files[0];
                    if (!file) return;

                    const v = validateFormat(file, isImage);
                    if (!v.ok) {
                        if (errorSpan) errorSpan.textContent = v.msg;
                        if (typeof toastr !== 'undefined') toastr.error(v.msg);
                        input.value = '';
                        return;
                    }

                    if (progressBar && progressFill) {
                        var id = getDraftId();
                        if (id) {
                            uploadWithProgress(file, isImage ? 'image' : 'video', progressBar, progressFill, errorSpan, uploadedSpan, token, input, id);
                        } else {
                            ensureDraft(function () {
                                uploadWithProgress(file, isImage ? 'image' : 'video', progressBar, progressFill, errorSpan, uploadedSpan, token, input, getDraftId());
                            });
                        }
                    }
                });
            }

            setupInput(imgInput, true, imgError, imgProgress, imgProgressFill, imgUploaded);
            setupInput(vidInput, false, vidError, vidProgress, vidProgressFill, vidUploaded);
        }
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', initRegistrationUpload);
    else
        initRegistrationUpload();
})();
