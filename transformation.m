% matlab/octave script  

Transofmrations

T(x0,y0) = 0,h
T(x1,y1) = w,0

-----------------------------------------------------


syms m11 m22  m12 m21 x0 x1 y0 y1 
syms h w
syms xMin yMin xMax yMax
syms dx dy
syms ml mr mb mt

wp = w-mr-ml
hp = h-mt-mb

p_nn = [xMin;yMin]
p_nx = [xMin;yMax]

p_xn = [xMax;yMin]
p_xx = [xMax;yMax]


pp_nn = [0;hp]
pp_nx = [0;0]

pp_xn = [wp;hp]
pp_xx = [wp;0]


T=[m11 m12;m21 m22]
D=[dx;dy]


sys = [D+T*p_nn - pp_nn ;D+T*p_nx - pp_nx ;D+T*p_xn - pp_xn ;D+T*p_xx - pp_xx;]
sys = [D+T*p_nn - pp_nn ;D+T*p_nx - pp_nx ;D+T*p_xn - pp_xn ;]

#unknowns are m11 m12 m21 m22 dx dy

unks = [m11 m12 m21 m22 dx dy]

for i=1:6
  row = sys(i)
  for j=1:6
     symbol = unks(i)
     coefs=sym2poly(row,symbol)
     
     

R=[0;h;w;0]

pp = [1 0 x0 0;0 1 0 y0;1 0 x1 0;0 1 0 y1]

inv(pp)*R

---
syms x_out y_out x_in y_in
p_out = [x_out ;y_out]
p_in = [x_in ;y_in]

Pout = T* Pin + D

Pout - D = T * Pin
p_in = T^-1 * (p_out-D)

p_in = expand(det(T) * inv(T) * (p_out-D) )