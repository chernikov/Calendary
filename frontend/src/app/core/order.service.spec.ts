import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { beforeEach, afterEach, describe, expect, it } from 'vitest';
import { OrderService } from './order.service';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('createOrder() posts the photo as multipart form data', () => {
    const photo = new File(['x'], 'photo.jpg', { type: 'image/jpeg' });
    service.createOrder(photo).subscribe();

    const req = httpMock.expectOne('/api/orders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    const uploaded = (req.request.body as FormData).get('photo') as File;
    expect(uploaded.name).toBe('photo.jpg');
    expect(uploaded.type).toBe('image/jpeg');
    req.flush({});
  });

  it('getOrder() fetches a single order by id', () => {
    service.getOrder('order-1').subscribe();

    const req = httpMock.expectOne('/api/orders/order-1');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('listOrders() fetches the customer order list', () => {
    service.listOrders().subscribe();

    const req = httpMock.expectOne('/api/orders');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('checkout() posts the delivery payload to the order checkout endpoint', () => {
    const delivery = {
      recipientName: 'Андрій',
      phone: '+380671234567',
      city: 'Київ',
      warehouseNumber: '1',
      warehouseAddress: 'вул. Хрещатик, 1',
    };
    service.checkout('order-1', delivery).subscribe();

    const req = httpMock.expectOne('/api/orders/order-1/checkout');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(delivery);
    req.flush({});
  });

  it('cancel() posts to the cancel endpoint with no body', () => {
    service.cancel('order-1').subscribe();

    const req = httpMock.expectOne('/api/orders/order-1/cancel');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush({});
  });

  it('downloadPdf() requests a blob response', () => {
    service.downloadPdf('order-1').subscribe();

    const req = httpMock.expectOne('/api/orders/order-1/pdf');
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob());
  });

  it('novaPoshtaWarehouses() passes the city as a query param', () => {
    service.novaPoshtaWarehouses('Львів').subscribe();

    const req = httpMock.expectOne((r) => r.url === '/api/nova-poshta/warehouses');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('city')).toBe('Львів');
    req.flush([]);
  });

  it('generateSheet() sends null instead of undefined when no photoId is provided', () => {
    service.generateSheet('order-1', 2, 'prompt-1', 'style-1').subscribe();

    const req = httpMock.expectOne('/api/orders/order-1/sheets/2/generate');
    expect(req.request.body).toEqual({ promptId: 'prompt-1', imageStyleId: 'style-1', photoId: null });
    req.flush({});
  });

  it('checkoutBatch() spreads orderIds and delivery fields into one payload', () => {
    const delivery = {
      recipientName: 'Андрій',
      phone: '+380671234567',
      city: 'Київ',
      warehouseNumber: '1',
      warehouseAddress: 'вул. Хрещатик, 1',
    };
    service.checkoutBatch(['order-1', 'order-2'], delivery).subscribe();

    const req = httpMock.expectOne('/api/orders/checkout-batch');
    expect(req.request.body).toEqual({ orderIds: ['order-1', 'order-2'], ...delivery });
    req.flush(null);
  });
});
