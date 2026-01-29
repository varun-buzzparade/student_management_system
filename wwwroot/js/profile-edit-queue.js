/**
 * Profile / Edit form: per-field AJAX updates with queued requests and Toastr feedback.
 *
 * Flow:
 * 1. On load, snapshot all [data-endpoint] field values as "original".
 * 2. On submit: prevent default, collect only changed fields.
 * 3. If none changed → toastr.info('No changes detected'); return.
 * 4. Disable Save, show "Saving...". For each changed field, POST to basePath/endpoint?value=...&id=... (id if userIdSelector set).
 * 5. Requests run one-at-a-time via RequestQueue to avoid concurrent updates to the same record.
 * 6. On success: toastr.success(result.message); update original snapshot; if UpdateDateOfBirth, refresh age field.
 * 7. On error: toastr.error. When all requests finish, re-enable Save.
 *
 * Config: formId, basePath, userIdSelector? (e.g. '#studentId'), ageFieldId? ('age').
 */
(function () {
    'use strict';

    if (typeof toastr !== 'undefined') {
        toastr.options = { closeButton: true, progressBar: true, positionClass: 'toast-top-right', timeOut: 3000 };
    }

    /**
     * Serializes async work: run one request at a time, then process next.
     * Used so multiple field updates to the same record don't overlap.
     */
    function RequestQueue() {
        this.queue = [];
        this.isProcessing = false;
    }
    RequestQueue.prototype.add = function (request) {
        this.queue.push(request);
        this.process();
    };
    RequestQueue.prototype.process = async function () {
        if (this.isProcessing || this.queue.length === 0) return;
        this.isProcessing = true;
        var request = this.queue.shift();
        try {
            await request();
        } catch (e) {
            if (typeof console !== 'undefined' && console.error) console.error('Request failed:', e);
        }
        this.isProcessing = false;
        this.process();
    };

    /**
     * opts: { formId, basePath, userIdSelector?, ageFieldId? }
     * basePath: e.g. '/Admin' or '/Student'. Endpoints like UpdateFullName, UpdateEmail, etc.
     */
    window.initProfileEditQueue = function (opts) {
        var form = document.getElementById(opts.formId);
        if (!form) return;
        var saveBtn = document.getElementById('saveBtn');
        var basePath = opts.basePath;
        var getUserId = opts.userIdSelector
            ? function () { var el = document.querySelector(opts.userIdSelector); return el ? el.value : null; }
            : function () { return null; };
        var ageFieldId = opts.ageFieldId || 'age';

        var originalValues = {};
        var endpointFields = document.querySelectorAll('[data-endpoint]');
        for (var i = 0; i < endpointFields.length; i++) {
            var f = endpointFields[i];
            originalValues[f.id] = f.value;
        }

        var requestQueue = new RequestQueue();

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var userId = getUserId();
            var fields = document.querySelectorAll('[data-endpoint]');
            var changedFields = [];
            for (var i = 0; i < fields.length; i++) {
                var field = fields[i];
                if (field.value !== originalValues[field.id]) changedFields.push(field);
            }

            if (changedFields.length === 0) {
                toastr.info('No changes detected');
                return;
            }

            saveBtn.disabled = true;
            saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
            var completed = 0;
            var total = changedFields.length;

            for (var i = 0; i < changedFields.length; i++) {
                (function (field) {
                    var endpoint = field.getAttribute('data-endpoint');
                    var value = field.value;
                    var url = basePath + '/' + endpoint + '?value=' + encodeURIComponent(value);
                    if (userId) url += '&id=' + encodeURIComponent(userId);

                    requestQueue.add(async function () {
                        try {
                            var response = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' } });
                            var result = await response.json();
                            completed++;
                            if (result.success) {
                                toastr.success(result.message);
                                originalValues[field.id] = value;
                                if (endpoint === 'UpdateDateOfBirth' && result.age != null) {
                                    var ageEl = document.getElementById(ageFieldId);
                                    if (ageEl) ageEl.value = result.age;
                                }
                            } else {
                                toastr.error(result.message || 'Update failed');
                            }
                        } catch (err) {
                            completed++;
                            toastr.error('Network error occurred');
                            if (typeof console !== 'undefined' && console.error) console.error(err);
                        }
                        if (completed === total) {
                            saveBtn.disabled = false;
                            saveBtn.innerHTML = 'Save Changes';
                        }
                    });
                })(changedFields[i]);
            }
        });
    };
})();
