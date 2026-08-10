const state = {
  token: localStorage.getItem('parkora_token') || '',
  user: JSON.parse(localStorage.getItem('parkora_user') || 'null'),
  events: [],
  categories: [],
  currentEvent: null,
  selectedSeats: new Set(),
  selectedParking: null,
  currentSeats: [],
  currentParking: []
};

const $ = (selector, scope = document) => scope.querySelector(selector);
const $$ = (selector, scope = document) => [...scope.querySelectorAll(selector)];

function formatMoney(value) {
  return new Intl.NumberFormat('en-LK', { style: 'currency', currency: 'LKR', maximumFractionDigits: 0 }).format(Number(value || 0));
}
function formatDate(date) {
  if (!date) return '—';
  return new Intl.DateTimeFormat('en-LK', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(`${date}T00:00:00`));
}
function formatDateTime(value) {
  if (!value) return '—';
  return new Intl.DateTimeFormat('en-LK', { day:'2-digit', month:'short', hour:'2-digit', minute:'2-digit' }).format(new Date(value));
}
function initials(name='U') { return name.split(/\s+/).filter(Boolean).slice(0,2).map(x=>x[0]).join('').toUpperCase() || 'U'; }
function escapeHtml(value='') { return String(value).replace(/[&<>'"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c])); }

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (options.body && !(options.body instanceof FormData)) headers['Content-Type'] = 'application/json';
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  const response = await fetch(path, { ...options, headers });
  if (response.status === 401) {
    clearSession();
    openAuth('login');
    throw new Error('Your session expired. Please sign in again.');
  }
  if (response.status === 204) return null;
  const text = await response.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if (!response.ok) {
      const message =
          data?.message ||
          data?.Message ||
          data?.title ||
          (typeof data === 'string'
              ? data
              : `Request failed (${response.status})`);
    throw new Error(message);
  }
  return data;
}

function toast(message, type='success', title='Done') {
  const el = document.createElement('div');
  el.className = `toast ${type}`;
  el.innerHTML = `<div class="toast-icon">${type === 'error' ? '!' : '✓'}</div><div><strong>${escapeHtml(title)}</strong><p>${escapeHtml(message)}</p></div>`;
  $('#toastContainer').appendChild(el);
  setTimeout(() => el.remove(), 3800);
}

function syncAuthUI() {
  document.body.classList.toggle('authenticated', !!state.token && !!state.user);
  document.body.classList.toggle('is-admin', state.user?.role === 'Admin');
  if (state.user) {
    $('#headerName').textContent = state.user.fullName;
    $('#headerRole').textContent = state.user.role;
    $('#headerAvatar').textContent = initials(state.user.fullName);
  }
}
function setSession(auth) {
  state.token = auth.token;
  state.user = { customerId: auth.customerId, fullName: auth.fullName, email: auth.email, role: auth.role };
  localStorage.setItem('parkora_token', state.token);
  localStorage.setItem('parkora_user', JSON.stringify(state.user));
  syncAuthUI();
}
function clearSession() {
  state.token = '';
  state.user = null;
  localStorage.removeItem('parkora_token');
  localStorage.removeItem('parkora_user');
  syncAuthUI();
}

function openModal(id) { $(`#${id}`).classList.remove('hidden'); document.body.style.overflow = 'hidden'; }
function closeModal(id) { $(`#${id}`).classList.add('hidden'); document.body.style.overflow = ''; }
function openAuth(tab='login') {
  openModal('authModal');
  setAuthTab(tab);
}
function setAuthTab(tab) {
  $$('[data-auth-tab]').forEach(b => b.classList.toggle('active', b.dataset.authTab === tab));
  $('#loginForm').classList.toggle('active-auth-form', tab === 'login');
  $('#registerForm').classList.toggle('active-auth-form', tab === 'register');
}

function showView(name) {
    if (['bookings', 'feedback', 'notifications', 'profile'].includes(name) && !state.token) { openAuth('login'); return; }
  if (name === 'admin' && state.user?.role !== 'Admin') { toast('Admin access is required.', 'error', 'Access denied'); return; }
  $$('.view').forEach(v => v.classList.remove('active-view'));
  $(`#view-${name}`)?.classList.add('active-view');
  $$('.nav-link').forEach(b => b.classList.toggle('active', b.dataset.view === name));
  window.scrollTo({ top: 0, behavior: 'smooth' });
    if (name === 'bookings') loadBookings();
    if (name === 'feedback') loadAllFeedback();
  if (name === 'notifications') loadNotifications();
  if (name === 'profile') loadProfile();
  if (name === 'admin') loadAdmin();
}

async function loadCategories() {
  try {
    state.categories = await api('/api/Categories');
    const options = state.categories.map(c => `<option value="${c.categoryId}">${escapeHtml(c.name)}</option>`).join('');
    $('#categoryFilter').insertAdjacentHTML('beforeend', options);
  } catch (e) { console.error(e); }
}

async function loadEvents() {
  const params = new URLSearchParams();
  const name = $('#eventSearch').value.trim();
  const date = $('#eventDateFilter').value;
  const categoryId = $('#categoryFilter').value;
  if (name) params.set('name', name);
  if (date) params.set('date', date);
  if (categoryId) params.set('categoryId', categoryId);
  $('#eventsGrid').innerHTML = '<div class="skeleton-card"></div><div class="skeleton-card"></div><div class="skeleton-card"></div>';
  try {
    state.events = await api(`/api/Events${params.toString() ? `?${params}` : ''}`);
    renderEvents();
  } catch (e) {
    $('#eventsGrid').innerHTML = `<div class="empty-state"><strong>Could not load events</strong>${escapeHtml(e.message)}</div>`;
  }
}

function renderEvents() {
  $('#eventCount').textContent = `${state.events.length} event${state.events.length === 1 ? '' : 's'} found`;
  if (!state.events.length) {
    $('#eventsGrid').innerHTML = '<div class="empty-state"><strong>No events found</strong>Try changing the search or filters.</div>';
    return;
  }
  const glows = ['#72f1b8','#6bd6ff','#a68cff','#ffb86b','#ff7d8e'];
  $('#eventsGrid').innerHTML = state.events.map((event, index) => {
    const d = new Date(`${event.eventDate}T00:00:00`);
    const day = String(d.getDate()).padStart(2,'0');
    const month = d.toLocaleDateString('en', { month:'short' }).toUpperCase();
    return `<article class="event-card">
      <div class="event-cover" style="--event-glow:${glows[index % glows.length]}">
        <div class="event-date-box"><strong>${day}</strong><span>${month}</span></div>
        <div class="event-cover-title"><span>${escapeHtml(event.categoryName)}</span><h3>${escapeHtml(event.name)}</h3></div>
      </div>
      <div class="event-card-body">
        <div class="event-meta"><span>⌖ ${escapeHtml(event.venueName)}</span><span>◷ ${escapeHtml(String(event.eventTime).slice(0,5))}</span></div>
        <div class="event-footer">
          <div class="price-block"><small>Ticket from</small><strong>${formatMoney(event.ticketPrice)}</strong><span class="availability ${event.availableSeats === 0 ? 'locked':''}">${event.availableSeats} seats available</span></div>
          <button
    class="btn ${event.availableSeats === 0
            ? 'btn-soft'
            : 'btn-primary'}"
    data-open-event="${event.eventId}"
    ${event.availableSeats === 0 ? 'disabled' : ''}>
    
    ${event.availableSeats === 0
            ? 'Sold out'
            : 'Book now'}
</button>
        </div>
      </div>
    </article>`;
  }).join('');
}

async function openEvent(eventId) {
  const event = state.events.find(e => e.eventId === Number(eventId)) || await api(`/api/Events/${eventId}`);
  state.currentEvent = event;
  state.selectedSeats.clear();
  state.selectedParking = null;
  if (!state.token) {
    openAuth('login');
    toast('Sign in first to choose seats and parking.', 'error', 'Login required');
    return;
  }
  $('#eventModalContent').innerHTML = `<div class="event-modal-hero"><span class="eyebrow">${escapeHtml(event.categoryName)}</span><h2>${escapeHtml(event.name)}</h2><p>${escapeHtml(event.description || 'Choose your seats and optional parking slot for this event.')}</p><div class="event-modal-meta"><span>⌖ ${escapeHtml(event.venueName)}</span><span>▣ ${formatDate(event.eventDate)}</span><span>◷ ${escapeHtml(String(event.eventTime).slice(0,5))}</span></div></div><div class="booking-content"><div class="booking-section"><h3>Select your seats</h3><p>Loading currently available seats…</p></div></div>`;
  openModal('eventModal');
  try {
    const [seats, parking] = await Promise.all([
      api(`/api/Seat/event/${event.eventId}`),
      api(`/api/ParkingSlot/event/${event.eventId}/available`)
    ]);
    state.currentSeats = seats;
    state.currentParking = parking;
    renderBookingSelector();
  } catch (e) {
    $('#eventModalContent').insertAdjacentHTML('beforeend', `<div class="empty-state"><strong>Could not load availability</strong>${escapeHtml(e.message)}</div>`);
  }
}

function renderBookingSelector() {
  const event = state.currentEvent;
  const seats = state.currentSeats;
  const parking = state.currentParking;
  $('#eventModalContent').innerHTML = `
    <div class="event-modal-hero"><span class="eyebrow">${escapeHtml(event.categoryName)}</span><h2>${escapeHtml(event.name)}</h2><p>${escapeHtml(event.description || 'Choose your seats and optional parking slot for this event.')}</p><div class="event-modal-meta"><span>⌖ ${escapeHtml(event.venueName)}</span><span>▣ ${formatDate(event.eventDate)}</span><span>◷ ${escapeHtml(String(event.eventTime).slice(0,5))}</span></div></div>
    <div class="booking-content">
      <div>
        <section class="booking-section">
          <h3>Select seats</h3><p>Pick one or more available seats.</p>
         <div class="booking-legend">
    <span><i class="legend-dot"></i>Available</span>
    <span><i class="legend-dot selected"></i>Selected</span>
    <span><i class="legend-dot booked"></i>Booked</span>
</div>
          <div class="screen"></div>
          <div class="seat-grid">
    ${seats.length ? seats.map(s => {
        const isBooked = s.status === 'Booked';

        return `
            <button
                class="seat
                    ${state.selectedSeats.has(s.seatId) ? 'selected' : ''}
                    ${isBooked ? 'booked' : ''}"
                ${isBooked ? 'disabled' : `data-seat-id="${s.seatId}"`}
                title="${isBooked
                ? 'Already booked'
                : `Row ${escapeHtml(s.seatRow)}, seat ${s.seatNumber}`}">

                ${escapeHtml(s.seatRow)}${s.seatNumber}

            </button>
        `;
    }).join('') :
          '<div class="empty-state"><strong>No seats found</strong></div>'}
</div>
        </section>
        <section class="booking-section" style="margin-top:28px">
          <h3>Reserve parking <span class="muted">(optional)</span></h3><p>Choose one parking slot for the same event.</p>
          <div class="parking-grid">${parking.length ? parking.map(p => `<button class="parking-slot ${state.selectedParking===p.parkingSlotId?'selected':''}" data-parking-id="${p.parkingSlotId}">⌁ ${escapeHtml(p.slotLabel)}</button>`).join('') : '<div class="empty-state"><strong>No parking available</strong>You can continue with seats only.</div>'}</div>
        </section>
      </div>
      <aside class="booking-summary">
        <span class="eyebrow">SUMMARY</span><h3>${escapeHtml(event.name)}</h3>
        <div class="summary-line"><span>Selected seats</span><strong id="seatCountSummary">${state.selectedSeats.size}</strong></div>
        <div class="summary-line"><span>Seat total</span><strong id="seatTotalSummary">${formatMoney(state.selectedSeats.size * event.ticketPrice)}</strong></div>
        <div class="summary-line"><span>Parking</span><strong id="parkingSummary">${state.selectedParking ? formatMoney(event.parkingFee) : 'Not selected'}</strong></div>
        <div class="summary-line summary-total"><span>Total</span><strong id="grandTotalSummary">${formatMoney((state.selectedSeats.size * event.ticketPrice) + (state.selectedParking ? event.parkingFee : 0))}</strong></div>
        <button class="btn btn-primary btn-block" id="confirmBookingBtn" ${state.selectedSeats.size ? '' : 'disabled'}>Confirm booking</button>
        <p class="muted" style="font-size:10px;line-height:1.5;margin-bottom:0">Payment can be completed immediately after booking.</p>
      </aside>
    </div>`;
}

function updateBookingSummary() {
  const event = state.currentEvent;
  $('#seatCountSummary').textContent = state.selectedSeats.size;
  $('#seatTotalSummary').textContent = formatMoney(state.selectedSeats.size * event.ticketPrice);
  $('#parkingSummary').textContent = state.selectedParking ? formatMoney(event.parkingFee) : 'Not selected';
  $('#grandTotalSummary').textContent = formatMoney((state.selectedSeats.size * event.ticketPrice) + (state.selectedParking ? event.parkingFee : 0));
  $('#confirmBookingBtn').disabled = state.selectedSeats.size === 0;
}

async function createBooking() {
  if (!state.user) return openAuth('login');
  const btn = $('#confirmBookingBtn');
  btn.disabled = true; btn.textContent = 'Creating booking…';
  try {
    const booking = await api('/api/bookings', {
      method:'POST',
      body: JSON.stringify({
        customerId: state.user.customerId,
        eventId: state.currentEvent.eventId,
        seatIds: [...state.selectedSeats],
        parkingSlotId: state.selectedParking
      })
    });
    closeModal('eventModal');
    toast(`Booking ${booking.bookingNumber} created successfully.`);
    showBookingSuccess(booking);
    await loadEvents();
  } catch (e) {
    toast(e.message, 'error', 'Booking failed');
    btn.disabled = false; btn.textContent = 'Confirm booking';
  }
}

function showBookingSuccess(booking) {
  $('#receiptContent').innerHTML = `<div class="receipt-paper"><div class="receipt-check">✓</div><span class="eyebrow">BOOKING CREATED</span><h2>${escapeHtml(booking.bookingNumber)}</h2><p>Your seats${booking.parking ? ' and parking' : ''} are reserved.</p><div class="receipt-details"><div class="summary-line"><span>Event</span><strong>${escapeHtml(booking.eventName)}</strong></div><div class="summary-line"><span>Seats</span><strong>${booking.seats.map(s=>escapeHtml(s.seatLabel)).join(', ')}</strong></div><div class="summary-line"><span>Parking</span><strong>${booking.parking ? escapeHtml(booking.parking.slotLabel) : 'None'}</strong></div><div class="summary-line summary-total"><span>Amount due</span><strong>${formatMoney(booking.totalAmount)}</strong></div></div><button class="btn btn-primary btn-block" data-pay-booking="${booking.bookingId}">Pay now</button><button class="btn btn-soft btn-block" style="margin-top:8px" data-view-bookings-after>Pay later</button></div>`;
  openModal('receiptModal');
}

async function payBooking(bookingId) {
  try {
    const payment = await api(`/api/bookings/${bookingId}/payment`, { method:'POST' });
    const receipt = await api(`/api/payments/${payment.paymentId}/receipt`);
    $('#receiptContent').innerHTML = `<div class="receipt-paper"><div class="receipt-check">✓</div><span class="eyebrow">PAYMENT SUCCESSFUL</span><h2>Payment complete</h2><p>Your receipt is ready.</p><div class="receipt-details"><div class="summary-line"><span>Booking</span><strong>${escapeHtml(receipt.bookingNumber)}</strong></div><div class="summary-line"><span>Customer</span><strong>${escapeHtml(receipt.customerName)}</strong></div><div class="summary-line"><span>Event</span><strong>${escapeHtml(receipt.eventName)}</strong></div><div class="summary-line"><span>Seats</span><strong>${receipt.seatLabels.map(escapeHtml).join(', ')}</strong></div><div class="summary-line"><span>Parking</span><strong>${escapeHtml(receipt.parkingSlotLabel || 'None')}</strong></div><div class="summary-line summary-total"><span>Paid</span><strong>${formatMoney(receipt.amountPaid)}</strong></div></div><button class="btn btn-primary btn-block" data-view-bookings-after>View my bookings</button></div>`;
    toast('Payment recorded successfully.');
  } catch (e) { toast(e.message, 'error', 'Payment failed'); }
}

async function loadBookings() {
  const box = $('#bookingsList');
  box.innerHTML = '<div class="skeleton-card"></div>';
  try {
      const [bookings, feedbacks] = await Promise.all([
          api(`/api/bookings/customer/${state.user.customerId}`),
          api('/api/Feedback')
      ]);

      const feedbackBookingIds = new Set(
          feedbacks
              .filter(f => f.customerId === state.user.customerId)
              .map(f => f.bookingId)
      );
    if (!bookings.length) { box.innerHTML = '<div class="empty-state"><strong>No bookings yet</strong>Explore an event and reserve your first seat.</div>'; return; }
    box.innerHTML = bookings.map(b => `<article class="list-card">
      <div><div class="booking-title"><div class="booking-icon">${escapeHtml(b.eventName?.[0] || 'E')}</div><div class="booking-copy"><h3>${escapeHtml(b.eventName)}</h3><p>${escapeHtml(b.bookingNumber)} · ${formatDate(b.eventDate)} · ${escapeHtml(String(b.eventTime).slice(0,5))}</p></div><span class="status-pill ${b.isPaid ? 'success':'warn'}">${b.isPaid ? 'PAID' : 'PAYMENT DUE'}</span></div>
      <div class="booking-detail-row"><span>Seats: ${b.seats.map(s=>escapeHtml(s.seatLabel)).join(', ') || '—'}</span><span>Parking: ${b.parking ? escapeHtml(b.parking.slotLabel) : 'None'}</span><span>Total: ${formatMoney(b.totalAmount)}</span><span>Status: ${escapeHtml(b.status)}</span></div></div>
      <div class="booking-actions">${!b.isPaid ? `<button class="btn btn-primary btn-sm" data-pay-booking="${b.bookingId}">Pay now</button>` : ''}<button class="btn btn-soft btn-sm" data-feedback-booking="${b.bookingId}">Feedback</button><button class="btn btn-danger btn-sm" data-cancel-booking="${b.bookingId}">Cancel</button></div>
    </article>`).join('');
  } catch(e) { box.innerHTML = `<div class="empty-state"><strong>Could not load bookings</strong>${escapeHtml(e.message)}</div>`; }
}

async function cancelBooking(id) {
  if (!confirm('Cancel this booking?')) return;
  try { await api(`/api/bookings/${id}`, { method:'DELETE' }); toast('Booking cancelled.'); loadBookings(); loadEvents(); }
  catch(e){ toast(e.message,'error','Cancellation failed'); }
}

async function loadNotifications() {
  const box = $('#notificationsList'); box.innerHTML = '<div class="skeleton-card"></div>';
  try {
    const rows = await api(`/api/Notifications/customer/${state.user.customerId}`);
    if (!rows.length) { box.innerHTML='<div class="empty-state"><strong>You are all caught up</strong>New booking and payment updates will appear here.</div>';return; }
    box.innerHTML = rows.map(n => `<article class="list-card notification-card ${n.isRead?'':'unread'}"><div class="notification-dot"></div><div class="notification-copy"><h4>${escapeHtml(n.title)}</h4><p>${escapeHtml(n.message)}</p><small>${formatDateTime(n.createdAt)} · ${escapeHtml(n.type)}</small></div>${n.isRead?'':`<button class="btn btn-soft btn-sm" data-read-notification="${n.notificationId}">Mark read</button>`}</article>`).join('');
  } catch(e){ box.innerHTML=`<div class="empty-state"><strong>Could not load notifications</strong>${escapeHtml(e.message)}</div>`; }
}

async function markNotification(id){ try{await api(`/api/Notifications/${id}/read`,{method:'PUT'});loadNotifications();}catch(e){toast(e.message,'error');} }

async function loadProfile() {
  try {
    const data = await api(`/api/Customers/${state.user.customerId}`);
    const c = data.customer;
    $('#profileAvatar').textContent = initials(c.fullName);
    $('#profileDisplayName').textContent = c.fullName;
    $('#profileDisplayEmail').textContent = c.email;
    $('#profileTotalBookings').textContent = data.totalBookings;
    $('#profileUpcomingBookings').textContent = data.upcomingBookings;
    $('#profileFullName').value = c.fullName;
    $('#profileEmail').value = c.email;
    $('#profilePhone').value = c.phone || '';
  } catch(e){ toast(e.message,'error','Profile error'); }
}

async function saveProfile(event) {
  event.preventDefault();
  try {
    const updated = await api(`/api/Customers/${state.user.customerId}`, { method:'PUT', body:JSON.stringify({fullName:$('#profileFullName').value.trim(), phone:$('#profilePhone').value.trim() || null}) });
    state.user.fullName = updated.fullName;
    localStorage.setItem('parkora_user', JSON.stringify(state.user));
    syncAuthUI(); loadProfile(); toast('Profile updated successfully.');
  } catch(e){ toast(e.message,'error','Update failed'); }
}

async function loadAdmin() {
  try {
      const [stats] = await Promise.all([
          api('/api/Dashboard/admin'),
          loadAdminEvents(),
          searchCustomers(),
          loadAdminFeedback()
      ]);
    const cards = [
      ['Total events',stats.totalEvents],['Bookings',stats.totalBookings],['Available seats',stats.availableSeatsSystemWide],['Parking occupied',stats.occupiedParkingSlots],['Revenue',formatMoney(stats.totalRevenue)],['Customers',stats.totalCustomers]
    ];
    $('#adminStats').innerHTML = cards.map(c=>`<div class="stat-card"><small>${escapeHtml(c[0])}</small><strong>${escapeHtml(c[1])}</strong></div>`).join('');
  } catch(e){ toast(e.message,'error','Admin dashboard error'); }
}
async function loadAdminEvents() {
  const events = await api('/api/Events');
  $('#adminEventsTable').innerHTML = `<table class="data-table"><thead><tr><th>Event</th><th>Date</th><th>Venue</th><th>Seats</th><th>Parking</th></tr></thead><tbody>${events.map(e=>`<tr><td><strong>${escapeHtml(e.name)}</strong><br><span class="muted">${escapeHtml(e.categoryName)}</span></td><td>${formatDate(e.eventDate)}</td><td>${escapeHtml(e.venueName)}</td><td>${e.availableSeats}/${e.totalSeats}</td><td>${e.availableParkingSlots}/${e.totalParkingSlots}</td></tr>`).join('')}</tbody></table>`;
}
async function searchCustomers() {
  if (state.user?.role !== 'Admin') return;
  const q = $('#customerSearch').value.trim();
  try {
    const rows = await api(`/api/Customers${q ? `?search=${encodeURIComponent(q)}`:''}`);
    $('#adminCustomers').innerHTML = rows.length ? rows.map(c=>`<div class="customer-row"><span class="avatar">${initials(c.fullName)}</span><div><strong>${escapeHtml(c.fullName)}</strong><small>${escapeHtml(c.email)}</small></div><span class="status-pill ${c.isActive?'success':''}">${c.isActive?'ACTIVE':'INACTIVE'}</span></div>`).join('') : '<div class="empty-state">No customers found.</div>';
  } catch(e){ $('#adminCustomers').innerHTML=`<div class="empty-state">${escapeHtml(e.message)}</div>`; }
}

async function openEventForm() {
  try {
    const [venues, categories] = await Promise.all([api('/api/Venue'), api('/api/Categories')]);
    $('#adminEventVenue').innerHTML = '<option value="">Select venue</option>' + venues.map(v=>`<option value="${v.venueId}">${escapeHtml(v.name)}</option>`).join('');
    $('#adminEventCategory').innerHTML = '<option value="">Select category</option>' + categories.map(c=>`<option value="${c.categoryId}">${escapeHtml(c.name)}</option>`).join('');
    openModal('eventFormModal');
  } catch(e){ toast(e.message,'error','Could not prepare event form'); }
}
async function createAdminEvent(event) {
  event.preventDefault();
  try {
    await api('/api/Events', { method:'POST', body:JSON.stringify({
      name:$('#adminEventName').value.trim(), venueId:Number($('#adminEventVenue').value), categoryId:Number($('#adminEventCategory').value), eventDate:$('#adminEventDate').value, eventTime:($('#adminEventTime').value.length===5 ? $('#adminEventTime').value+':00' : $('#adminEventTime').value), ticketPrice:Number($('#adminTicketPrice').value), parkingFee:Number($('#adminParkingFee').value), description:$('#adminEventDescription').value.trim()||null, seatRows:Number($('#adminSeatRows').value), seatsPerRow:Number($('#adminSeatsPerRow').value), parkingSlotCount:Number($('#adminParkingCount').value)
    })});
    closeModal('eventFormModal'); $('#eventCreateForm').reset(); toast('Event published successfully.'); loadAdmin(); loadEvents();
  } catch(e){ toast(e.message,'error','Event creation failed'); }
}

function openFeedback(bookingId){ $('#feedbackBookingId').value=bookingId;$('#feedbackRating').value='5';$$('#ratingPicker button').forEach(b=>b.classList.toggle('active',Number(b.dataset.rating)<=5));$('#feedbackComment').value='';openModal('feedbackModal'); }
async function submitFeedback(event){event.preventDefault();try{await api('/api/Feedback',{method:'POST',body:JSON.stringify({bookingId:Number($('#feedbackBookingId').value),rating:Number($('#feedbackRating').value),comment:$('#feedbackComment').value.trim()||null})});closeModal('feedbackModal');toast('Thanks for sharing your feedback.');}catch(e){toast(e.message,'error','Feedback failed');}}

let searchTimer;
function bindEvents() {
  document.addEventListener('click', e => {
    const view = e.target.closest('[data-view]')?.dataset.view;
    if (view) showView(view);
    const openEventId = e.target.closest('[data-open-event]')?.dataset.openEvent;
    if (openEventId) openEvent(openEventId);
    const close = e.target.closest('[data-close]')?.dataset.close;
    if (close) closeModal(close);
    const pay = e.target.closest('[data-pay-booking]')?.dataset.payBooking;
    if (pay) payBooking(pay);
    const cancel = e.target.closest('[data-cancel-booking]')?.dataset.cancelBooking;
    if (cancel) cancelBooking(cancel);
    const feedback = e.target.closest('[data-feedback-booking]')?.dataset.feedbackBooking;
    if (feedback) openFeedback(feedback);
    const read = e.target.closest('[data-read-notification]')?.dataset.readNotification;
    if (read) markNotification(read);
    if (e.target.closest('[data-view-bookings-after]')) { closeModal('receiptModal'); showView('bookings'); }

    const seatBtn = e.target.closest('[data-seat-id]');
    if (seatBtn) {
      const id = Number(seatBtn.dataset.seatId);
      state.selectedSeats.has(id) ? state.selectedSeats.delete(id) : state.selectedSeats.add(id);
      seatBtn.classList.toggle('selected'); updateBookingSummary();
    }
    const parkBtn = e.target.closest('[data-parking-id]');
    if (parkBtn) {
      const id = Number(parkBtn.dataset.parkingId);
      state.selectedParking = state.selectedParking === id ? null : id;
      $$('[data-parking-id]').forEach(b=>b.classList.toggle('selected',Number(b.dataset.parkingId)===state.selectedParking));
      updateBookingSummary();
    }
    if (e.target.closest('#confirmBookingBtn')) createBooking();
  });

    $('#refreshAdminFeedbackBtn')
        .addEventListener('click', loadAdminFeedback);
  $('#loginBtn').addEventListener('click',()=>openAuth('login'));
  $('#registerBtn').addEventListener('click',()=>openAuth('register'));
  $('#logoutBtn').addEventListener('click',()=>{clearSession();showView('home');toast('You have been signed out.');});
  $$('[data-auth-tab]').forEach(b=>b.addEventListener('click',()=>setAuthTab(b.dataset.authTab)));
  $('#exploreBtn').addEventListener('click',()=>$('#eventsSection').scrollIntoView({behavior:'smooth'}));
  $('#myBookingsHeroBtn').addEventListener('click',()=>showView('bookings'));
  $('#clearFiltersBtn').addEventListener('click',()=>{$('#eventSearch').value='';$('#eventDateFilter').value='';$('#categoryFilter').value='';loadEvents();});
  $('#eventSearch').addEventListener('input',()=>{clearTimeout(searchTimer);searchTimer=setTimeout(loadEvents,350)});
  $('#eventDateFilter').addEventListener('change',loadEvents); $('#categoryFilter').addEventListener('change',loadEvents);
  $('#profileForm').addEventListener('submit',saveProfile);
  $('#openEventFormBtn').addEventListener('click',openEventForm); $('#eventCreateForm').addEventListener('submit',createAdminEvent); $('#refreshAdminEventsBtn').addEventListener('click',loadAdminEvents);
  $('#customerSearch').addEventListener('input',()=>{clearTimeout(searchTimer);searchTimer=setTimeout(searchCustomers,350)});
  $('#feedbackForm').addEventListener('submit',submitFeedback);
  $$('#ratingPicker button').forEach(b=>b.addEventListener('click',()=>{const r=Number(b.dataset.rating);$('#feedbackRating').value=r;$$('#ratingPicker button').forEach(x=>x.classList.toggle('active',Number(x.dataset.rating)<=r));}));
  $$('#ratingPicker button').forEach(b=>b.classList.add('active'));

  $('#loginForm').addEventListener('submit', async e => {
    e.preventDefault();
    try {
      const auth = await api('/api/Auth/login',{method:'POST',body:JSON.stringify({email:$('#loginEmail').value.trim(),password:$('#loginPassword').value})});
      setSession(auth); closeModal('authModal'); $('#loginForm').reset(); toast(`Welcome back, ${auth.fullName.split(' ')[0]}!`); if(auth.role==='Admin') showView('admin');
    } catch(err){toast(err.message,'error','Sign in failed');}
  });
  $('#registerForm').addEventListener('submit', async e => {
    e.preventDefault();
    try {
      const auth = await api('/api/Auth/register',{method:'POST',body:JSON.stringify({fullName:$('#registerName').value.trim(),email:$('#registerEmail').value.trim(),phone:$('#registerPhone').value.trim(),password:$('#registerPassword').value})});
      setSession(auth); closeModal('authModal'); $('#registerForm').reset(); toast('Account created successfully.');
    } catch(err){toast(err.message,'error','Registration failed');}
  });

  $('#themeBtn').addEventListener('click',()=>{document.body.classList.toggle('light');const light=document.body.classList.contains('light');localStorage.setItem('parkora_theme',light?'light':'dark');$('#themeBtn').textContent=light?'☀':'☾';});
  $$('.modal-backdrop').forEach(m=>m.addEventListener('click',e=>{if(e.target===m)closeModal(m.id)}));
  document.addEventListener('keydown',e=>{if(e.key==='Escape')$$('.modal-backdrop:not(.hidden)').forEach(m=>closeModal(m.id))});
}

async function init() {
  if (localStorage.getItem('parkora_theme') === 'light') { document.body.classList.add('light'); $('#themeBtn').textContent='☀'; }
  syncAuthUI(); bindEvents();
  await Promise.all([loadCategories(), loadEvents()]);
}

document.addEventListener('DOMContentLoaded', init);

async function loadAllFeedback() {

    const box = $('#allFeedbackList');

    box.innerHTML = '<div class="skeleton-card"></div>';

    try {

        const feedbacks = await api('/api/Feedback');

        if (!feedbacks.length) {

            box.innerHTML = `
                <div class="empty-state">
                    <strong>No feedback yet</strong>
                    Customer reviews will appear here.
                </div>
            `;

            return;
        }

        box.innerHTML = feedbacks.map(f => `
            <article class="list-card">

                <div>

                    <div class="booking-title">

                        <div class="booking-icon">
                            ${escapeHtml(f.customerName?.[0] || 'C')}
                        </div>

                        <div class="booking-copy">

                            <h3>${escapeHtml(f.eventName)}</h3>

                            <p>
                                ${escapeHtml(f.customerName)}
                                · ${formatDateTime(f.createdAt)}
                            </p>

                        </div>

                    </div>

                    <div class="booking-detail-row">

                        <span>
                            Rating:
                            ${'★'.repeat(f.rating)}
                            ${'☆'.repeat(5 - f.rating)}
                        </span>

                        <span>
                            ${escapeHtml(f.comment || 'No comment')}
                        </span>

                    </div>

                </div>

                <span class="status-pill success">
                    ${f.rating}/5
                </span>

            </article>
        `).join('');

    }
    catch (e) {

        box.innerHTML = `
            <div class="empty-state">
                <strong>Could not load feedback</strong>
                ${escapeHtml(e.message)}
            </div>
        `;
    }
}

async function loadAdminFeedback() {

    const box = $('#adminFeedbackList');

    box.innerHTML = '<div class="skeleton-card"></div>';

    try {

        const feedbacks = await api('/api/Feedback');

        if (!feedbacks.length) {

            box.innerHTML = `
                <div class="empty-state">
                    <strong>No feedback yet</strong>
                    Customer feedback will appear here.
                </div>
            `;

            return;
        }

        box.innerHTML = feedbacks.map(f => `
            <article class="list-card">

                <div>

                    <div class="booking-title">

                        <div class="booking-icon">
                            ${escapeHtml(f.customerName?.[0] || 'C')}
                        </div>

                        <div class="booking-copy">

                            <h3>${escapeHtml(f.eventName)}</h3>

                            <p>
                                ${escapeHtml(f.customerName)}
                                · ${formatDateTime(f.createdAt)}
                            </p>

                        </div>

                    </div>

                    <div class="booking-detail-row">

                        <span>
                            Rating:
                            ${'★'.repeat(f.rating)}
                            ${'☆'.repeat(5 - f.rating)}
                        </span>

                        <span>
                            ${escapeHtml(f.comment || 'No comment')}
                        </span>

                    </div>

                </div>

                <span class="status-pill success">
                    ${f.rating}/5
                </span>

            </article>
        `).join('');

    }
    catch (e) {

        box.innerHTML = `
            <div class="empty-state">
                <strong>Could not load feedback</strong>
                ${escapeHtml(e.message)}
            </div>
        `;
    }
}
