package org.dinbur.ml.rl.monopoly.game.properties;


import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import org.dinbur.ml.rl.monopoly.utils.RuntimeTypeAdapterFactory;

import java.util.List;

public class MonopolyProperty {

    public static final Gson GSON = createPropertyGsonParser();

    private String name;
    private int position;
    private int price;
    private List<Integer> rent;
    private int mortgage;
    private int houseCost;
    private int hotelCost;
    private String group;


    public String getName() {
        return name;
    }

    public int getPosition() {
        return position;
    }

    public int getPrice() {
        return price;
    }

    public List<Integer> getRent(){
        return rent;
    }

    public int getMortgage() {
        return mortgage;
    }

    public int getHotelCost() {
        return hotelCost;
    }

    public int getHouseCost() {
        return houseCost;
    }

    public String getGroup() {
        return group;
    }

    @Override
    public String toString() {
        return "name: " + name + "@" + position + " | " + price;
    }

    public int computeRent(int propertiesOwned){
        verifyWithinRange(propertiesOwned);
        return rent.get(propertiesOwned - 1);
    }

    private void verifyWithinRange(int propertiesOwned) {
        if (propertiesOwned < 0 || rent.size() < propertiesOwned) {
            throw new IllegalArgumentException("Invalid parameter 'propertiesOwned': " + String.valueOf(propertiesOwned) + " for " + this.toString());
        }
    }

    private static Gson createPropertyGsonParser() {

        RuntimeTypeAdapterFactory<MonopolyProperty> typeAdapterFactory = RuntimeTypeAdapterFactory
                .of(MonopolyProperty.class, "type")
                .registerSubtype(RailwayProperty.class, "railways")
                .registerSubtype(UtilityProperty.class, "utilities")
                .registerSubtype(MonopolyProperty.class, "standard");

        return new GsonBuilder()
                .registerTypeAdapterFactory(typeAdapterFactory)
                .create();
    }
}
