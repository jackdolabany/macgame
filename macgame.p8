pico-8 cartridge // http://www.pico-8.com
version 41
__lua__
-- variables

function _init()
  player={
    sp=1,
    x=10,
    y=10,
    w=8,
    h=8,
    flipped=false,
    dx=0,
    dy=0,
    max_dx=4,
    max_dy=4,
    acc=0.5,
    boost=4,
    anim=0,
    running=false,
    jumping=false,
    landed=false,
    sliding = false,
    jump_pressed = false,
    is_platform_below = false, -- on update, scan for a jump thru platofrm below the player
    health=3,
    invincible = false,
    invincible_timer=0
  }
  
  -- testing
  player.x = 930
  
  enemies={
    { sp=19,x=2*8,y=11*8,w=8,
     h=8,flipped=false,dist=0,
     max_dist=32,dx=1,dy=0,
     update=enemy_walk,anim=0,
     sp_count=0,sp_total=2,alive=true },
    
    -- cat boss
    { sp=5,x=120*8,y=128-5*8,w=16,
     h=16,flipped=false,dist=0,
     max_dist=32,dx=0,dy=0,
     update=enemy_nomove,anim=0,
     sp_count=0,sp_total=3,alive=true,
     is_boss=true },
    
    
    { sp=35,x=30,y=20,w=8,h=8,
    flipped=false,dist=0,
    max_dist=32,dx=0,dy=1.3,
    update=enemy_fly,anim=0,
    sp_count=0,sp_total=2,alive=true }
    
  }

  gravity=0.4
  friction=0.75
  
  -- simple camera
  cam_x = 0
  
  -- map limits
  map_start = 0
  map_end = 1024
end


-->8
-- update and draw

function _update()

  if player.health > 0 then
    player_update()
  end
  
  player_animate()
  
  for e in all(enemies) do
    if e.alive then
      e.update(e)
    end
  end

  for e in all(enemies) do
    if e.alive then
      enemy_animate(e)
    end
  end
  
  if player.health > 0 and not player.invincible then
    for e in all(enemies) do
      check_enemy_collide(player,e)
    end
  end

  -- fall down pit
  if player.health > 0 then
   if player.y > 128 then
    sfx(3)
    player.health = 0
    player.alive = false
   end
  end

  -- simple camera
  cam_x = player.x-64+(player.w/2)
  
  if cam_x < map_start then
    cam_x = map_start
  end
  
  if cam_x > map_end-128 then
    cam_x = map_end-128
  end
  
  camera(cam_x, 0)
  
end

function _draw()
  cls(12)
  map(0,0)
  
  if player.health > 0 then
    spr(player.sp, player.x, player.y, 1, 1, player.flipped)
  end
  
  for e in all(enemies) do
    if e.alive then
    	if e.is_boss then
      spr(e.sp, e.x, e.y, 2, 2, e.flipped)
     else
    	 spr(e.sp, e.x, e.y, 1, 1, e.flipped)
    	end
    end
  end

  -- draw hud
  local hearts = player.health
  local heartx = 0
  while hearts > 0 do
    spr(33,heartx,0,1,1,false)
    heartx += 8
    hearts-=1
  end
end

-->8
-- collisions
function collide_map(obj, aim, flag)
    -- obj = table needs x,y,w,h
    -- aim = left, right, up, down
  local x = obj.x 
  local y = obj.y
  local w = obj.w
  local h = obj.h
    
  local x1 = 0
  local x2 = 0
  local y1 = 0
  local y2 = 0
    
 	if aim == "left" then
  		x1=x-1
	  	y1=y
	  	x2=x
	  	y2=y+h-1
  elseif aim == "right" then
		  x1=x+w-1
		  y1=y
		  x2=x+w
		  y2=y+h-1
	 elseif aim == "up" then
	  	x1=x+2
	  	y1=y-1
	  	x2=x+w-3
		  y2=y
	 elseif aim == "down" then
		  x1=x+2
	  	y1=y+h
	  	x2=x+w-3
	  	y2=y+h
	 end
	
	 -- pixels to tiles
	 x1/=8
	 y1/=8
	 x2/=8
	 y2/=8
	 
	 if fget(mget(x1,y1), flag)
	   or fget(mget(x2,y1), flag)
	   or fget(mget(x1,y2), flag)
	   or fget(mget(x2,y2), flag) then
	     return true
	 else
	   return false
	 end
	 
end

-->8
-- player
function player_update()

  -- environment checks
  if collide_map(player,"down",2) then
    -- sand
    friction=0.3
    player.boost = 2  
  elseif collide_map(player,"down",3) then
    -- ice
    friction = 0.95
    player.boost = 4
  else
  	 -- normal
  	 friction = 0.75
  	 player.boost = 4
  end
  
  -- invincible timer
  if player.invincible then
    if player.invincible_timer <= time() then
      player.invincible = false
    end
  end
  
  --physics
  player.dy+=gravity
  player.dx*=friction

  --controls
  if btn(⬅️) then
    player.dx-=player.acc
    player.running=true
    player.sliding=false
    player.flipped=true
  end
  if btn(➡️) then
    player.dx+=player.acc
    player.running=true
    player.sliding=false
    player.flipped=false
  end

  --slide
  if player.running
    and not btn(⬅️)
    and not btn(➡️)
    and not player.falling
    and not player.jumping then
    player.running = false
    player.sliding = true
  end

  --jump
  if btnp(🅾️)
  and player.landed 
  and not player.jump_pressed then
    sfx(0)
    player.dy-=player.boost
    player.landed=false
    player.sliding=false
    player.jump_pressed = true
  end
  
  if not btn(🅾️) then
    player.jump_pressed = false
  end

  --check collision up and down
  if player.dy>0 then
    player.falling=true
    player.landed=false
    player.jumping=false
    
    player.dy=limit_speed(player.dy,player.max_dy)

    -- collision with regular solid 
    -- blocks
    local falling_thru_solid = collide_map(player,"down",1)
    local falling_thru_platform = collide_map(player,"down",0)

    if falling_thru_solid
     or (falling_thru_platform and player.is_platform_below) then
      player.landed=true
      player.falling=false
      player.dy=0
      -- tweak player height in 
      -- case they fell through
      -- ground a little
      player.y-=((player.y+player.h+1)%8)-1
    end
    
  elseif player.dy<0 then
    player.jumping=true
    if collide_map(player,"up",1) then
      player.dy=0
    end
  end

  -- scan for platforms below the player
  local obj = {
  	x = player.x,
  	y = player.y + player.h - 2,
  	h = player.h,
  	w = player.w
  }
  
  player.is_platform_below = 
    collide_map(obj,"down",0)

  --check collision left and right
  if player.dx<0 then

    player.dx=limit_speed(player.dx,player.max_dx)

    if collide_map(player,"left",1) then
      player.dx=0
    end
  elseif player.dx>0 then

    player.dx=limit_speed(player.dx,player.max_dx)

    if collide_map(player,"right",1) then
      player.dx=0
    end
  end

  -- slightly sliding is not
  -- sliding so we see the idle
  -- animation.
		if player.dx < 0.3 and player.dx > -0.3 then
		  player.running = false
		  player.sliding = false
		end

  -- stop the player if they are 
  -- nearly stopped
  -- so you don't get a weird
  -- late 1 pixel movement
		if player.dx < 0.1 and player.dx > -0.1 then
		  player.dx = 0
		end

  player.x+=player.dx
  player.y+=player.dy

  -- bound the player in the map
	 if player.x < map_start then
	   player.x = map_start
	 elseif player.x > map_end - player.w then
	   player.x = map_end - player.w
	 end
	 
end

function player_animate()
  if player.jumping then
    player.sp=17
  elseif player.falling then
    player.sp=18
  elseif player.running then
    if time()-player.anim>.2 then
      player.anim=time()
      player.sp+=1
      if player.sp>2 then
        player.sp=1
      end
    end
  elseif player.sliding then
    player.sp = 3
  else --player idle
    player.sp = 1
  end
end

function limit_speed(num,maximum)
  return mid(-maximum,num,maximum)
end

function value_in_range(val,low,high)
  return val >= low and val <=high
end

function rect_intersects(x1,y1,h1,w1,x2,y2,h2,w2)
  local x_overlap = value_in_range(x1,x2,x2+w2)
                    or value_in_range(x2,x1,x1+w1)
  local y_overlap = value_in_range(y1,y2,y2+h2)
                    or value_in_range(y2,y1,y1+h1)
  return x_overlap and y_overlap
end 

function check_enemy_collide(player,enemy)
  
  if player.health == 0 then
    return
  end
  
  if not enemy.alive then 
    return
  end
  
  if rect_intersects(
    player.x,player.y,player.h,player.w,
    enemy.x,enemy.y,enemy.h,enemy.w) then
    
    if player.y + player.h - 4 < enemy.y then
      -- player jumped on the enemy
      enemy.alive = false
      sfx(3)
    else
      -- player takes a hit
		    player.health-=1
		    if player.health == 0 then
		      -- death
		      sfx(2)
		    else
		      -- take a hit
		      sfx(1)
		      player.invincible = true
		      player.invincible_timer = time() + 1.2
		    end
		  end
  end
end

-->8
-- enemies
function enemy_walk(e)
  e.x += e.dx
  e.dist += e.dx
  if (e.dist >= e.max_dist or e.dist <= 0) e.dx = -e.dx

	 e.flipped = e.dx > 0
end

function enemy_fly(e)
  e.y += e.dy
  e.dist += e.dy
  if (e.dist >= e.max_dist or e.dist <= 0) e.dy = -e.dy
 
  e.flipped = e.x < player.x
end

function enemy_nomove(e)
  e.flipped = e.x < player.x
end

function enemy_animate(e)
  if time()-e.anim>.15 then
    e.anim=time()
    
    e.sp_count+=1
    
    sprite_size = 1
    if e.is_boss then
     sprite_size=2
    end
    
    if e.sp_count < e.sp_total then
      e.sp+=sprite_size
    else
      e.sp_count=0
      e.sp-=(sprite_size * (e.sp_total-1))
    end
    
 
  end
end
-->8
--bosses

__gfx__
0000000000000000000000000000000000000000000000000000000000000000000000000000d0000000d0000000000000000000000000000000000000000000
000000000009990000099900000999000000000000000000000000000000000000000000000d6d00000d6d000000000000000000000000000000000000000000
007007000061919000619190006191900000000000000000000000000000d0000000d000000d6dd000dd6d000000000000000000000000000000000000000000
00077000909666009096660090966600000000000000d0000000d000000d6d00000d6d00000d766ddd667d000000000000000000000000000000000000000000
0007700096999000969990009699900000000000000d6d00000d6d00000d6dd000dd6d0000d67b3777b376d00000000000000000000000000000000000000000
0070070009699600096996000969969000000000000d6dd000dd6d00000d766ddd667d0000d67337773376d00000000000000000000000000000000000000000
0000000000909000000990000090090000000000000d766ddd667d0000d67b3777b376d00d67ee72227ee76d0000000000000000000000000000000000000000
000000000990990000099000009909900000000000d67b3777b376d000d67337773376d00d6777572757776d0000000000000000000000000000000000000000
000000000009990000099900000000000000000000d67337773376d00d67ee72227ee76d0d6777755577776d0000000000000000000000000000000000000000
00000000006191900061919000000000000000000d67ee72227ee76d0d6777572757776d0d6677777777766d0000000000000000000000000000000000000000
00000000909666009096660011100110111001100d6777572757776d0d6777755577776d00d66777777766d00000000000000000000000000000000000000000
00000000969990009699900071701111717011110d6777755577776d0d6677777777766d000d666666666d000000000000000000000000000000000000000000
000000000969960009699600111111111111111100d6ddd666ddd6d000d6ddd666ddd6d000d6ddd666ddd6d00000000000000000000000000000000000000000
0000000000909000009090000011111000011110000d666d6d666d00000d666d6d666d00000d666d6d666d000000000000000000000000000000000000000000
000000000900900000909000010110100100101000d67776d67776d000d67776d67776d000d67776d67776d00000000000000000000000000000000000000000
0000000090090000000909000010010010010001000ddddd0ddddd00000ddddd0ddddd00000ddddd0ddddd000000000000000000000000000000000000000000
00000000000000000000000000999900009999000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000ee0ee00000000070898907008989000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000e88e88e0000000077999977009999000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000e88888e00000000670aa076000aa0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000e88888e0000000006999760069997600000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000e888e00000000000aaaa0077aaaa770000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000e8e000000000000099900700999070000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000e0000000000000008000000080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000007000070000007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000070070000700700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000770000007700007007000000000070000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000770000007700000070000700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000070070000700700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000007000070007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0dddddd00bbbb0bb44444444cccccccc000000000000000000500505055055055550555555555000000000000000000000000000000000000000000000000000
d4444445b0bbbbbb45444444cccccccc777777770000000005555555555005555550055555555500000000000000000000000000000000000000000000000000
d4dddd45bb3bbbb344444444cccccccc755555570000000005555555550550055505500555555550000000000000000000000000000000000000000000000000
d4d44545b4bb3b4b44444454cccccccc556555550000000055555555505555505055555055555550000000000000000000000000000000000000000000000000
d4d4454544b4b44444444444cccccccc555555650000000005555555055555500555555055555550000000000000000000000000000000000000000000000000
d45555454444444444544444cccccccc555555550000000005555555055555050555550555555550000000000000000000000000000000000000000000000000
d44444454444544544444444cccccccc565565550000000055555555550550555505505555555550000000000000000000000000000000000000000000000000
055555504444444444444444cccccccc555555550000000055555555555055555550555555555555000000000000000000000000000000000000000000000000
5555555400b0bbb00bb0bbb00bb0bb00039999999999999999999990999999990000000011111111111111111111111100000001100000000000000000000000
599999440bbbbbbbbbbbbbbbbbbbbbb0bb3999999a999a99999a993b9aaaaaaa00099000166766711aaaabb11777766100000001100000000000000000000000
599994940bbbb3bbbbbb3bbbbbbbbbb0bb39999999999999a999993b9aaaaaaa0009900016766761daabbbb1d776666100000001100000000000000000000000
59994944bbbb333b3bb33333b333b3bbb339a99999999999999993bb9aaaaaaa00777700166676611abbbbb11766666100000001100000000000000000000000
59949494bbb34433333343443343333b3333999a9a99999a99a93b339999999909999990166766711abbbbb11766666100000001100000000000000000000000
59494944b3344454434443544443443b44433a9999999a9999994453aaaa9aaa06666660167667611bbbbbb11666666100000001100000000000000000000000
54949494b5444444444444444544444b4544499999a99999a9994444aaaa9aaa99999999166666611bbbbbb11666666100000001100000000000000000000000
444444444444444444445444444444444444444999999a9999945444aaaa9aaa4444444411111111dbbbbbb1d666666100000001100000000000000000000000
00000000000000000000000011111111037777777777777777777773111111114444444467777777dbbbbba1d66666a1000000a1100000000000000000000000
00000000000000000000000017777661bb377677767776777776773b1ddddddd48888888655555571bbbbb911666669100000091100000000000000000000000
0000000000000000000000001776666dbb377777777777776777773b1ddddddd48888888656777571bbbbbb11666666100000001100000000000000000000000
00000000000000000000000017666661b337677777777777777773bb1ddddddd48888888656557571bbbbb31166666d100000001100000000000000000000000
00000000000000000000000017666661333377767677777677673b331111111144444444656557571bbbbb31166666d100000001100000000000000000000000
00000000000000000000000016666661443336777777767777774453dddd1ddd8888488865666657dbbbb331d6666dd100000001100000000000000000000000
00000000000000000000000016666661454447777767777767774444dddd1ddd88884888655555571bb33331166dddd100000001100000000000000000000000
0000000000000000000000001666666d444444477777767777745444dddd1ddd8888488866666666111111111111111100000001100000000000000000000000
0000000000000000000000001a66666d000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000019666661000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000016666661000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000166666d1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000166666d1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000016666ddd000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000166dddd1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000011111111000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
__gff__
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001030300000000000000000000000000030303030707070000000000000000000b0b0b000b0b0b00000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
__map__
4343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343
4343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343
4343434343434343434343434343434343434340404343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434243434343434343434343434343434343
4343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343424243434343434343434343434343434343
4343434343434343434343434343434343404043434343434343434343434343435043434343434343434343434343434350434343434340404043434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343424243434343434343434343434343434343
434343434343435c5d635b434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434342424243434343434343434343434343434343
434340404343436c6d736b434343434040434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434141414343434343434343434343434343434343434343434343434343434343434343434343434343434342424243434343434343434343434343434343
4343434343575957595768596859685968675967594343434343434343504343434343434343434343434343434343434343434343434343434141414143434341414242424141414141414141424343434343434343434343434343434343434343434343434343434343434242424243434343434343434343434343434343
43434343435757575757686868686868686767676740404343435b4343434343434343434343434343434343434343434343435043434343424242424243434342424242424242424242424242424242434343434343434343434343434343434343434343434343434343424242424243434343434343434343434343434343
43434343435759575957685968596859686759675943434343436b4343434343434343434343434343434343434343434343434343434242424242424243434342424242424242424242424242424242424242434343434343434343434343434343434343434343434342424242424243434343434343434343434343434343
4343434343575757575768686868686868676767674343434350505c5d635d43434343434343434343434343434343434343434343424242424242424243434342424242424242424242424242424242424242424242434342424243434343434343434343434343424242424242424243434343434343434343434343434343
4343434343575b5759576859685a685968675b67595043434350506c5d736d43434343434343434343434343434343434343434342424242424242424243434342424242424242424242424242424242424242424242434342424242424243434343434343434342424242424242424243434343434343434343434343434343
5252525252576b5757576868686a686868676b67675252525252525252525254555555555652525264656565656565656566525242424242424242424243434342424242424242424242424242424242424242424242434342424242424242424243434342424242424242424242424243434343434343434343434343434343
4242424242484848484848484848484848484848484242424242424242424242424242424242424242424242424242424242424242424242424242424243434342424242424242424242424242424242424242424242434342424242424242424242424242424242424242424242424242424242424242424242424242424242
4242424242484848484848484848484848484848484242424242424242424242424242424242424242424242424242424242424242424242424242424243434342424242424242424242424242424242424242424242434342424242424242424242424242424242424242424242424242424242424242424242424242424242
4242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424242424243434342424242424242424242424242424242424242424242434342424242424242424242424242424242424242424242424242424242424242424242424242424242
4343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343
4343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343434343
0042420000000000000043434343434300000000000000000000000000000043430000000000000000000000434343434343434343000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
__sfx__
0004000000000000002620004750087500f75017750097500570005700030000a0000e0000d1000d1000d1000d1002710031100361000270000700007002620026200272002820021200031001a1002d10000000
0004000000000076500d65010650046500065030600001000b100000000000003000060000c000150000c00006000040000400000000000000000000000000000000000000000000000000000000000000000000
000600000000000000000000e6501c6502265022650226501c65017650106500c6500c65012650156500965004650026500065002650016500065000650006500000000000000000000000000000000000000000
0007000000000001500d150041500c150011000010000100001000210001100011000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00060000040500d050100500000000000000000c1000d1000d1000d1000d1000d1000d1000d1000d100000000d1000d100000000d1000d1000d1000d100000000000000000000000000000000000000000000000
000a0000260502a0502c0001d000270002f0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00080000165301854021500225001350005500155001a5000550013500215000f500165000050000500215002850030500375003a50013500185001f50022500215001b500115000050000500005000050000500
